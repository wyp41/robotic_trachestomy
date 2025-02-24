using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RosMessageTypes.Geometry;
using RosMessageTypes.Moveit;
using RosMessageTypes.UrRobotDriver;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class TrajectoryPlanner : MonoBehaviour
{
    // Hardcoded variables
    const int k_NumRobotJoints = 6;
    const float k_JointAssignmentWait = 0.1f;
    const float k_PoseAssignmentWait = 0.1f;

    public int plan = 0;

    int num = 0;

    // Variables required for ROS communication
    [SerializeField]
    string m_RosServiceName = "ur5_moveit";

    public int finished = 1;
    public string RosServiceName { get => m_RosServiceName; set => m_RosServiceName = value; }

    public List<Vector3> m_Target;

    public Transform tool2ee;
    public Vector3 ee_pos;

    [SerializeField]
    GameObject m_ur5;
    public GameObject ur5 { get => m_ur5; set => m_ur5 = value; }
    [SerializeField]
    // GameObject m_Target;
    public GameObject Target;

    public Transform target_parent;

    PoseMsg[] waypoints;


    // Articulation Bodies
    ArticulationBody[] m_JointArticulationBodies;



    // ROS Connector
    ROSConnection m_Ros;

    /// <summary>
    ///     Find all robot joints in Awake() and add them to the jointArticulationBodies array.
    ///     Find left and right finger joints and assign them to their respective articulation body objects.
    /// </summary>
    void Start()
    {
        // Get ROS connection static instance
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.RegisterRosService<MoverServiceRequest, MoverServiceResponse>(m_RosServiceName);

        m_JointArticulationBodies = new ArticulationBody[k_NumRobotJoints];

        m_Target = new List<Vector3>();

        finished = 1;

        int defDyanmicVal = 10;
        var linkName = string.Empty;
        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            linkName += SourceDestinationPublisher.LinkNames[i];
            m_JointArticulationBodies[i] = m_ur5.transform.Find(linkName).GetComponent<ArticulationBody>();
            // m_JointArticulationBodies[i].gameObject.AddComponent<JointControl>();
            // JointControl current = m_JointArticulationBodies[i].GetComponent<JointControl>();
            m_JointArticulationBodies[i].jointFriction = defDyanmicVal;
            m_JointArticulationBodies[i].angularDamping = defDyanmicVal;
            // current.speed = 0.01f;
            // current.acceleration = 0.01f;
        }
    }

    void Update()
    {
        //var quat = target_parent.transform.rotation;
        //print(quat.eulerAngles);
        //CurrentJointConfig();
        if (plan != 0){
            finished = 0;
            var request = new MoverServiceRequest();
            request.joints_input = CurrentJointConfig();

            var quat = Quaternion.Inverse(ur5.transform.rotation) * target_parent.transform.rotation;
            //quat = quat * Quaternion.Euler(90, 0, 0);
            quat = quat * Quaternion.Inverse(tool2ee.localRotation);
            //print(quat.eulerAngles);

            // Target Pose
            for (int i = 0; i < m_Target.Count; i++){
                PoseMsg waypoint = new PoseMsg
                {
                    position = (m_Target[i] + quat * ee_pos).To<FLU>(),
                    //position = (m_Target[i]).To<FLU>(),

                    // The hardcoded x/z angles assure that the gripper is always positioned above the target cube before grasping.
                    orientation = quat.To<FLU>()
                };
                waypoints[i] = waypoint;
            }
            request.target_pose = waypoints;

            m_Ros.SendServiceMessage<MoverServiceResponse>(m_RosServiceName, request, TrajectoryResponse);
            
            plan = 0;
        }
    }

    void sleep(float time)
    {
        for (int i = 0; i < (int) 10000000 * time; i++)
        {
            continue;
        }
    }

    /// <summary>
    ///     Get the current values of the robot's joint angles.
    /// </summary>
    /// <returns>NiryoMoveitJoints</returns>
    MoveitJointsMsg CurrentJointConfig()
    {
        var joints = new MoveitJointsMsg();

        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            joints.joints[i] = m_JointArticulationBodies[i].jointPosition[0];
            //print(joints.joints[i] * Mathf.Rad2Deg);
        }

        return joints;
    }

    /// <summary>
    ///     Create a new MoverServiceRequest with the current values of the robot's joint angles,
    ///     the target cube's current position and rotation, and the targetPlacement position and rotation.
    ///     Call the MoverService using the ROSConnection and if a trajectory is successfully planned,
    ///     execute the trajectories in a coroutine.
    /// </summary>
    public void ServiceStart()
    {
        num = 0;
        //m_Target.Clear();
        //m_Target = new List<Vector3>();
        m_Target = Target.GetComponent<SmoothCurveHandler>().target;
        print(m_Target.Count);
        waypoints = new PoseMsg[m_Target.Count];
        if (plan == 0){
            plan = 1;
            ur5.GetComponent<RosSubscriberExample>().starts = 1;
        }
        else{
            plan = 0;
        }
    }

    void TrajectoryResponse(MoverServiceResponse response)
    {
        if (response.trajectories.Length > 0)
        {
            Debug.Log("Trajectory returned.");
            StartCoroutine(ExecuteTrajectories(response));
        }
        else
        {
            Debug.LogError("No trajectory returned from MoverService.");
        }
    }

    /// <summary>
    ///     Execute the returned trajectories from the MoverService.
    ///     The expectation is that the MoverService will return four trajectory plans,
    ///     PreGrasp, Grasp, PickUp, and Place,
    ///     where each plan is an array of robot poses. A robot pose is the joint angle values
    ///     of the six robot joints.
    ///     Executing a single trajectory will iterate through every robot pose in the array while updating the
    ///     joint values on the robot.
    /// </summary>
    /// <param name="response"> MoverServiceResponse received from niryo_moveit mover service running in ROS</param>
    /// <returns></returns>
    IEnumerator ExecuteTrajectories(MoverServiceResponse response)
    {
        int num = 0;
        if (response.trajectories != null)
        {
            // For every trajectory plan returned
            for (var poseIndex = 0; poseIndex < response.trajectories.Length; poseIndex++)
            {
                // For every robot pose in trajectory plan
                foreach (var t in response.trajectories[poseIndex].joint_trajectory.points)
                {
                    num++;
                    if (num < 2){
                        continue;
                    }
                    var jointPositions = t.positions;
                    var result = jointPositions.Select(r => (float)r * Mathf.Rad2Deg).ToArray();

                    // Set the joint values for every joint
                    for (var joint = 0; joint < m_JointArticulationBodies.Length; joint++)
                    {
                        var joint1XDrive = m_JointArticulationBodies[joint].xDrive;
                        //print(joint1XDrive.driveType);
                        joint1XDrive.damping = 1000;   
                        joint1XDrive.stiffness = 11000;
                        joint1XDrive.target = result[joint];
                        m_JointArticulationBodies[joint].xDrive = joint1XDrive;
                    }

                    // Wait for robot to achieve pose for all joint assignments
                    yield return new WaitForSeconds(k_JointAssignmentWait);
                }
                // Wait for the robot to achieve the final pose from joint assignment
                yield return new WaitForSeconds(k_PoseAssignmentWait);
            }
        }
        finished = 1;
        //plan = 1;
    }
}
