//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.UrRobotDriver
{
    [Serializable]
    public class MoverServiceRequest : Message
    {
        public const string k_RosMessageName = "ur_robot_driver/MoverService";
        public override string RosMessageName => k_RosMessageName;

        public MoveitJointsMsg joints_input;
        public Geometry.PoseMsg[] target_pose;

        public MoverServiceRequest()
        {
            this.joints_input = new MoveitJointsMsg();
            this.target_pose = new Geometry.PoseMsg[0];
        }

        public MoverServiceRequest(MoveitJointsMsg joints_input, Geometry.PoseMsg[] target_pose)
        {
            this.joints_input = joints_input;
            this.target_pose = target_pose;
        }

        public static MoverServiceRequest Deserialize(MessageDeserializer deserializer) => new MoverServiceRequest(deserializer);

        private MoverServiceRequest(MessageDeserializer deserializer)
        {
            this.joints_input = MoveitJointsMsg.Deserialize(deserializer);
            deserializer.Read(out this.target_pose, Geometry.PoseMsg.Deserialize, deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.joints_input);
            serializer.WriteLength(this.target_pose);
            serializer.Write(this.target_pose);
        }

        public override string ToString()
        {
            return "MoverServiceRequest: " +
            "\njoints_input: " + joints_input.ToString() +
            "\ntarget_pose: " + System.String.Join(", ", target_pose.ToList());
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
