﻿/// Copyright by Rob Jellinghaus.  All rights reserved.

using Holofunk.Core;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

namespace Holofunk.HandPose
{
    /// <summary>
    /// This behavior expects to inhabit a "FloatingHandTextPanel" GameObject, with a peer
    /// SolverHandler that determines the handedness this is tracking, and a child
    /// Text object with a Text Mesh component that will be updated with text about this
    /// hand.
    /// </summary>
    public class HandPoseVisualizer : MonoBehaviour
    {
        private HandPoseClassifier _classifier = new HandPoseClassifier();

        public Handedness handedness = Handedness.Left;

        // Update is called once per frame
        void Update()
        {
            var handJointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>();
            var gazeProvider = this.GetComponent<GazeProvider>();
            //var handedness = GetComponent<SolverHandler>().TrackedHandedness;
            //handedness = this.GetComponent<SolverHandler>().TrackedHandedness;
            //print(gazeProvider);
            if (handJointService != null && handJointService.IsHandTracked(handedness))
            {
                print("2");
                _classifier.Recalculate(handJointService, gazeProvider, handedness);

                // and update the text
                float knuckleDist = _classifier.GetSumPairwiseKnuckleDistances();
                float fingertipDist = _classifier.GetSumPairwiseFingertipDistances();
                float fingertipAlt = _classifier.GetSumFingertipAltitudes();
                print(_classifier.GetHandPose());
//                textMesh.text =
//$@"Finger poses: {Pose(Finger.Thumb)}, {Pose(Finger.Index)}, {Pose(Finger.Middle)}, {Pose(Finger.Ring)}, {Pose(Finger.Pinky)}
//Joint lin: {Colin(Finger.Thumb),0:f3}, {Colin(Finger.Index),0:f3}, {Colin(Finger.Middle),0:f3}, {Colin(Finger.Ring),0:f3}, {Colin(Finger.Pinky),0:f3}
//Finger pair co-ext: {Ext(Finger.Thumb)}, {Ext(Finger.Index)}, {Ext(Finger.Middle)}, {Ext(Finger.Ring)}
//Finger pair lin: {PairColin(Finger.Thumb),0:f3}, {PairColin(Finger.Index),0:f3}, {PairColin(Finger.Middle),0:f3}, {PairColin(Finger.Ring),0:f3}
//Eye->knuck lin: {EyeColin(Finger.Index),0:f3}, {EyeColin(Finger.Middle),0:f3}, {EyeColin(Finger.Ring),0:f3}
//Tip / knuck: {fingertipDist,0:f3} / {knuckleDist,0:f3} = {fingertipDist / knuckleDist,0:f3} (alt {fingertipAlt,0:f3}, ratio {fingertipAlt / knuckleDist,0:f3})
//Hand pose: {_classifier.GetHandPose()}";

                string Pose(Finger finger)
                {
                    FingerPose pose = _classifier.GetFingerPose(finger);
                    return pose == FingerPose.Extended ? "Ext" : pose == FingerPose.Curled ? "Curl" : "?";
                }

                float Colin(Finger finger) => _classifier.GetFingerJointColinearity(finger); // short for "Colinearity"

                string Ext(Finger finger) // short for "Extension"
                {
                    FingerPairExtension ext = _classifier.GetFingerPairExtension(finger);
                    return ext == FingerPairExtension.ExtendedTogether ? "Ext" : ext == FingerPairExtension.NotExtendedTogether ? "Not" : "?";
                }

                float PairColin(Finger finger) => _classifier.GetFingerPairColinearity(finger);

                float EyeColin(Finger finger) => _classifier.GetFingerEyeColinearity(finger);
            }
        }
    }
}
