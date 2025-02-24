// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MixedReality.Toolkit.SpatialManipulation
{
    /// <summary>
    /// Implements a generic movelogic that works for most/all XRI interactors,
    /// assuming a well-defined attachTransform. 
    /// 
    /// Usage:
    /// When a manipulation starts, call Setup.
    /// Call Update any time to update the move logic and get a new rotation for the object.
    /// </summary>
    public class MoveLogic : ManipulationLogic<Vector3>
    {
        private Vector3 attachToObject;
        private Vector3 objectLocalAttachPoint;
        private Vector3 startTransform;
        private Vector3 updateTransform;
        private Vector3 worldTransform;
        Thread mThread;
        string connectionIP = "127.0.0.1";
        int connectionPort = 25002;
        IPAddress localAdd;
        TcpListener listener;
        TcpClient client;
        static bool running = false;

        // static bool comm = false;
        bool stopped;
        bool start;
        static bool move = true;
        int len;

        /// <inheritdoc />
        public override void Setup(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable, MixedRealityTransform currentTarget)
        {
            base.Setup(interactors, interactable, currentTarget);

            Vector3 attachCentroid = GetAttachCentroid(interactors, interactable);

            startTransform = attachCentroid;
            updateTransform = Vector3.zero;
            worldTransform = Vector3.zero;

            attachToObject = currentTarget.Position - attachCentroid;
            objectLocalAttachPoint = Quaternion.Inverse(currentTarget.Rotation) * (attachCentroid - currentTarget.Position);
            objectLocalAttachPoint = objectLocalAttachPoint.Div(currentTarget.Scale);
            if (!running)
            {
                ThreadStart ts = new ThreadStart(GetInfo);
                mThread = new Thread(ts);
                mThread.Start();
            }
            
        }

        /// <inheritdoc />
        public override Vector3 Update(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable, MixedRealityTransform currentTarget, bool centeredAnchor)
        {
            base.Update(interactors, interactable, currentTarget, centeredAnchor);

            Vector3 attachCentroid = GetAttachCentroid(interactors, interactable);   

            startTransform = attachCentroid;

            if (centeredAnchor)
            {
                return attachCentroid + attachToObject;
            }
            else
            {
                if (move)
                {
                    Vector3 scaledLocalAttach = Vector3.Scale(objectLocalAttachPoint, currentTarget.Scale);
                    Vector3 worldAttachPoint = currentTarget.Rotation * scaledLocalAttach + currentTarget.Position;
                    return currentTarget.Position + (attachCentroid - worldAttachPoint);
                }
                else
                {
                    //objectLocalAttachPoint = Quaternion.Inverse(currentTarget.Rotation) * (attachCentroid - currentTarget.Position);
                    //objectLocalAttachPoint = objectLocalAttachPoint.Div(currentTarget.Scale);
                    Vector3 scaledLocalAttach = Vector3.Scale(objectLocalAttachPoint, currentTarget.Scale);
                    Vector3 worldAttachPoint = currentTarget.Rotation * scaledLocalAttach + currentTarget.Position;
                    worldTransform = worldAttachPoint;
                    updateTransform = attachCentroid - worldAttachPoint;
                    len = 1;
                    return currentTarget.Position;
                }
            }
        }

        private Vector3 GetAttachCentroid(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable)
        {
            // TODO: This uses the attachTransform ONLY, which can possibly be
            // unstable/imprecise (see GrabInteractor, etc.) Old version used to use the interactor
            // transform in the case where there was only one interactor, and the attachTransform
            // when there were 2+. The interactor should stabilize its attachTransform
            // to get a similar effect. Possibly, we should stabilize grabs on the thumb, or some
            // other technique.

            Vector3 sumPos = Vector3.zero;
            int count = 0;
            foreach (IXRSelectInteractor interactor in interactors)
            {
                sumPos += interactor.GetAttachTransform(interactable).position;
                count++;
            }

            return sumPos / Mathf.Max(1, count);
        }

        void GetInfo()
        {
            localAdd = IPAddress.Parse(connectionIP);
            listener = new TcpListener(IPAddress.Any, connectionPort);
            listener.Start();
            client = listener.AcceptTcpClient();

            stopped = false;
            running = true;
            // comm = true;
            while (running)
            {
                try
                {
                    if (stopped)
                    {
                        client = listener.AcceptTcpClient();
                    }
                    len = 0;
                    SendAndReceiveData();
                    stopped = false;
                }
                catch
                {
                    client.Close();
                    stopped = true;
                    //running = false;
                }
            }
            listener.Stop();
        }

        void SendAndReceiveData()
        {
            NetworkStream nwStream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];

            //---receiving Data from the Host----
            int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize); //Getting data in Bytes from Python
            string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead); //Converting byte data to string


            if (dataReceived != null)
            {
                //---Using received data---
                if (dataReceived.StartsWith("q"))
                {
                    running = false;
                }
                if (dataReceived.EndsWith("m"))
                {
                    move = !move;
                }
                if (dataReceived.EndsWith("s"))
                {
                    move = false;
                }


                //---Sending Data to Host----
                byte[] myWriteBuffer = Encoding.ASCII.GetBytes(len.ToString() + updateTransform.ToString() + objectLocalAttachPoint.ToString()); //Converting string to byte data
                nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python
                // len = 0;
            }
        }
    }
}