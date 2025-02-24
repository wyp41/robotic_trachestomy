using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UrRobotDriver;
using System.Numerics;

public class RosSubscriberExample : MonoBehaviour
{
    const int k_NumRobotJoints = 6;

    const float k_JointAssignmentWait = 0.1f;

    public int starts = 1;

    int wait = 0;

    [SerializeField]
    GameObject m_ur5;
    public GameObject ur5 { get => m_ur5; set => m_ur5 = value; }

    public float[] joints;

    // Articulation Bodies
    ArticulationBody[] m_JointArticulationBodies;

    // ROS Connector
    ROSConnection m_Ros;

    void Start()
    {
        m_Ros = ROSConnection.GetOrCreateInstance();
        
        m_JointArticulationBodies = new ArticulationBody[k_NumRobotJoints];

        joints = new float[k_NumRobotJoints];

        var linkName = string.Empty;
        int defDyanmicVal = 10;
        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            linkName += SourceDestinationPublisher.LinkNames[i];
            m_JointArticulationBodies[i] = m_ur5.transform.Find(linkName).GetComponent<ArticulationBody>();
            m_JointArticulationBodies[i].gameObject.AddComponent<JointControl>();
            // JointControl current = m_JointArticulationBodies[i].GetComponent<JointControl>();
            m_JointArticulationBodies[i].jointFriction = defDyanmicVal;
            m_JointArticulationBodies[i].angularDamping = defDyanmicVal;
            // current.speed = 0.1f;
            // current.acceleration = 0.1f;
        }

        ROSConnection.GetOrCreateInstance().Subscribe<MoveitJointsMsg>("start_joints", joints_share);
    }

    void Update()
    {
        CurrentJointConfig();
    }

    void CurrentJointConfig()
    {

        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            joints[i] = m_JointArticulationBodies[i].jointPosition[0];
            //print(joints.joints[i] * Mathf.Rad2Deg);
        }
    }

    void joints_share(MoveitJointsMsg joints)
    {
        if (starts == 1){
            for (int i = 0; i < 6; i++)
            {
                // int joint = 0;
                var joint1XDrive = m_JointArticulationBodies[i].xDrive;
                joint1XDrive.forceLimit = 1000;
                joint1XDrive.stiffness = 30000;
                joint1XDrive.damping = 100;
                joint1XDrive.target = (float)joints.joints[i] * Mathf.Rad2Deg;
                //print(joint1XDrive.target);
                m_JointArticulationBodies[i].xDrive = joint1XDrive;
            }
            starts = 0;
        }
    }
}