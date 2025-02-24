// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause


using MixedReality.Toolkit.Subsystems;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandTarget : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The XRNode on which this hand is located.")]
    private XRNode handNode = XRNode.LeftHand;

    /// <summary> The XRNode on which this hand is located. </summary>
    public XRNode HandNode { get => handNode; set => handNode = value; }

    private HandsAggregatorSubsystem handsSubsystem;

    // Transformation matrix for each joint.
    private List<Matrix4x4> jointMatrices = new List<Matrix4x4>();

    /// <summary>
    /// A Unity event function that is called when the script component has been enabled.
    /// </summary>
    protected void OnEnable()
    {
        Debug.Assert(handNode == XRNode.LeftHand || handNode == XRNode.RightHand, $"HandVisualizer has an invalid XRNode ({handNode})!");

        handsSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();

        if (handsSubsystem == null)
        {
            StartCoroutine(EnableWhenSubsystemAvailable());
        }
        else
        {
            for (int i = 0; i < (int)TrackedHandJoint.TotalJoints; i++)
            {
                jointMatrices.Add(new Matrix4x4());
            }
        }
    }

    /// <summary>
    /// A Unity event function that is called to draw Unity editor gizmos that are also interactable and always drawn.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!enabled) { return; }
        
        // Query all joints in the hand.
        if (handsSubsystem == null || !handsSubsystem.TryGetEntireHand(handNode, out IReadOnlyList<HandJointPose> joints))
        {
            return;
        }

        for (int i = 0; i < joints.Count; i++)
        {
            HandJointPose joint = joints[i];
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(joint.Position, joint.Forward * 0.01f);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(joint.Position, joint.Right * 0.01f);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(joint.Position, joint.Up * 0.01f);
        }
    }

    /// <summary>
    /// Coroutine to wait until subsystem becomes available.
    /// </summary>
    private IEnumerator EnableWhenSubsystemAvailable()
    {
        yield return new WaitUntil(() => XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>() != null);
        OnEnable();
    }

    /// <summary>
    /// A Unity event function that is called every frame, if this object is enabled.
    /// </summary>
    private void Update()
    {
        // Query all joints in the hand.
        if (handsSubsystem == null || !handsSubsystem.TryGetEntireHand(handNode, out IReadOnlyList<HandJointPose> joints))
        {
            return;
        }

        
        GetJoints(joints);
        //print(jointMatrices[11]);
        var matrix = jointMatrices[10];
        var position = new Vector3(matrix[0,3], matrix[1,3], matrix[2,3]);
        //print(position);
        transform.position = position;
    }

    private void GetJoints(IReadOnlyList<HandJointPose> joints)
    {
        for (int i = 0; i < joints.Count; i++)
        {
            // Skip joints with uninitialized quaternions.
            // This is temporary; eventually the HandsSubsystem will
            // be robust enough to never give us broken joints.
            if (joints[i].Rotation.Equals(new Quaternion(0, 0, 0, 0)))
            {
                continue;
            }

            //print(joints[i].Position);
            //print(joints[i].Rotation);
            // Fill the matrices list with TRSs from the joint poses.
            jointMatrices[i] = Matrix4x4.TRS(joints[i].Position, joints[i].Rotation.normalized, Vector3.one * joints[i].Radius);
        }
        // Draw the joints.
        // Graphics.DrawMeshInstanced(jointMesh, 0, jointMaterial, jointMatrices);
    }
}
