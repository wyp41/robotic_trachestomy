using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Solvers;
using MixedReality.Toolkit.SpatialManipulation;
using RosMessageTypes.Geometry;
using RosMessageTypes.UrRobotDriver;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEngine.GraphicsBuffer;

public class velocity_ctrl : MonoBehaviour
{
    // Hardcoded variables
    const int k_NumRobotJoints = 6;
    const float k_JointAssignmentWait = 0.001f;
    //const float k_PoseAssignmentWait = 0.1f;
    int run = 0;

    // Variables required for ROS communication
    string m_RosServiceName = "velocity_ctrl";
    public string RosServiceName { get => m_RosServiceName; set => m_RosServiceName = value; }

    public Vector3 pos_velocity;
    public Vector3 quat_velocity;

    [SerializeField]
    GameObject m_ur5;
    public GameObject ur5 { get => m_ur5; set => m_ur5 = value; }

    public GameObject Effector;

    // Articulation Bodies
    ArticulationBody[] m_JointArticulationBodies;

    // ROS Connector
    ROSConnection m_Ros;
    // Start is called before the first frame update
    void Start()
    {
        // Get ROS connection static instance
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.RegisterRosService<velocityServiceRequest, velocityServiceResponse>(m_RosServiceName);

        m_JointArticulationBodies = new ArticulationBody[k_NumRobotJoints];

        pos_velocity = new Vector3(0, 0, 0);
        quat_velocity = new Vector3(0, 0, 0);

        int defDyanmicVal = 100;
        var linkName = string.Empty;
        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            linkName += SourceDestinationPublisher.LinkNames[i];
            m_JointArticulationBodies[i] = m_ur5.transform.Find(linkName).GetComponent<ArticulationBody>();
            m_JointArticulationBodies[i].jointFriction = defDyanmicVal;
            m_JointArticulationBodies[i].angularDamping = defDyanmicVal;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (run != 0)
        {
            // x -> z, y -> -x, z -> y
            var pos_force = Effector.GetComponent<ObjectManipulator>().updateTranslate;

            // rx -> -rx, ry -> rz, rz -> -ry
            var quat_force = Effector.GetComponent<ObjectManipulator>().updateRotation;
            var selected = Effector.GetComponent<ObjectManipulator>().is_selected;
            if (selected != 0)
            {
                if (run == 1)
                {
                    pos_force = Vector3.Scale(pos_force, new Vector3(0.001f, 0.001f, 0.001f));
                    quat_force = new Quaternion(0, 0, 0, 1);
                }
                //print(pos_force);
                if (run == 2)
                {
                    pos_force = new Vector3(0, 0, 0);
                }
            }
            else
            {
                pos_force = Vector3.Scale(pos_velocity, new Vector3(0.00001f, 0.00001f, 0.00001f));
                quat_force = Quaternion.Euler(quat_velocity);
            }

            var current_pos = this.transform.position;
            var current_quat = this.transform.rotation;

            var right_handed_force = pos_force + current_pos;
            var right_handed_quat = quat_force * current_quat;

            var vel_request = new MoveitJointsMsg();
            for (var i = 0; i < 6; i++)
            {
                vel_request.joints[i] = 0;
            }
            PoseMsg target_pose = new PoseMsg
            {
                position = right_handed_force.To<FLU>(),
                orientation = right_handed_quat.To<FLU>()
            };

            var request = new velocityServiceRequest();
            request.joints_input = CurrentJointConfig();
            request.velocity_input = vel_request;
            request.target_pose = target_pose;
            m_Ros.SendServiceMessage<velocityServiceResponse>(m_RosServiceName, request, JointVelocityResponse);
        }
    }

    MoveitJointsMsg CurrentJointConfig()
    {
        var joints = new MoveitJointsMsg();

        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            joints.joints[i] = m_JointArticulationBodies[i].jointPosition[0];
        }

        return joints;
    }

    void JointVelocityResponse(velocityServiceResponse response)
    {
        if (response.velocity_output.joints.Length > 0)
        {
            //Debug.Log("velocity returned.");
            StartCoroutine(ExecuteJointVelocity(response));
        }
        else
        {
            Debug.LogError("No trajectory returned from MoverService.");
        }
    }

    IEnumerator ExecuteJointVelocity(velocityServiceResponse response)
    {
        var joint_velocity = response.velocity_output.joints;
        var result = joint_velocity.Select(r => (float)r * Mathf.Rad2Deg).ToArray();
        //print(result[0]);
        //print(result[1]);
        //print(result[2]);
        for (var joint = 0; joint < m_JointArticulationBodies.Length; joint++)
        {
            var jointXDrive = m_JointArticulationBodies[joint].xDrive;
            //jointXDrive.driveType = ArticulationDriveType.Velocity;
            //jointXDrive.targetVelocity = (float) joint_velocity[joint];
            jointXDrive.target = result[joint];
            m_JointArticulationBodies[joint].xDrive = jointXDrive;
        }
        yield return new WaitForSeconds(k_JointAssignmentWait);
    }
    public void ServiceStart()
    {
        if (run == 0)
        {
            run = 1;
        }
        else if (run == 1)
        {
            run = 2;
        }
        else
        {
            run = 0;
        }
    }
}
