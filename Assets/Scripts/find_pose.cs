using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System;
using UnityEngine.UIElements;
using UnityEditor.Experimental;

public class find_pose : MonoBehaviour
{
    Thread mThread;
    public GameObject ee_link;
    public string connectionIP = "127.0.0.1";
    public int connectionPort = 25001;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;

    bool running;
    bool stopped;

    string rot_dir = "z";
    int finished = 1;
    float modified = 0;
    // Start is called before the first frame update
    void Start()
    {
        rot_dir = "z";
    }

    public void start_finding()
    {
        ee_link.GetComponent<TrajectoryPlanner>().ServiceStart();
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
    }

    void GetInfo()
    {
        localAdd = IPAddress.Parse(connectionIP);
        listener = new TcpListener(IPAddress.Any, connectionPort);
        listener.Start();
        client = listener.AcceptTcpClient();

        stopped = false;
        running = true;
        while (running)
        {
            try
            {
                if (stopped)
                {
                    client = listener.AcceptTcpClient();
                }
                SendAndReceiveData();
                stopped = false;
            }
            catch
            {
                client.Close();
                stopped = true;
            }
        }
        listener.Stop();
    }

    void SendAndReceiveData()
    {
        //print("in");
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        //---receiving Data from the Host----
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize); //Getting data in Bytes from Python
        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead); //Converting byte data to string
        //print(dataReceived);
        if (dataReceived != null)
        {
            //---Using received data--- 
            if (dataReceived == "x")
            {
                rot_dir = "x";
            }
            else if (dataReceived == "y")
            {
                rot_dir = "y";
            }
            else if (dataReceived == "z")
            {
                rot_dir = "z";
            }
            else
            {
                modified = float.Parse(dataReceived);
            }

            //print(finished);
            //---Sending Data to Host----
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(finished.ToString()); //Converting string to byte data
            nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python
        }
    }

    // Update is called once per frame
    void Update()
    {
        finished = ee_link.GetComponent<TrajectoryPlanner>().finished;
        if (modified != 0)
        {
            var cur_rot = this.transform.localRotation.eulerAngles;
            if (rot_dir == "z")
            {
                cur_rot.z += modified;
            }
            else if (rot_dir == "x")
            {
                cur_rot.x += modified;
            }
            else if (rot_dir == "y")
            {
                 cur_rot.y += modified;
            }
            this.transform.localRotation = Quaternion.Euler(cur_rot);
            modified = 0;
            ee_link.GetComponent<TrajectoryPlanner>().plan = 1;
            finished = 0;
        }
    }
}
