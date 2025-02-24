using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.IO;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Text;

public class ImageReceiver : MonoBehaviour
{
    public string connectionIP = "127.0.0.1";
    public int connectionPort = 25005;
    public RawImage rawImage; // Assign this in the Unity Editor

    Texture2D texture;
    Thread mThread;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;

    bool running;
    bool stopped;
    bool start;

    byte[] imageData;

    void Start()
    {
        start = false;
        texture = new Texture2D(2, 2);
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
    }

    private void Update()
    {
        if (start)
        {
            //print(texture);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            rawImage.texture = texture;
        }
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
        NetworkStream nwStream = client.GetStream();

        //---receiving Data from the Host----

        BinaryReader reader = new BinaryReader(nwStream);

        //print("in");

        int imageSize = IPAddress.NetworkToHostOrder(reader.ReadInt32());

        imageData = reader.ReadBytes(imageSize);

        start = true;

        //print(imageData.Length);

        //texture = new Texture2D(2, 2);

        // 读取图片数据

        byte[] myWriteBuffer = Encoding.ASCII.GetBytes("hello"); //Converting string to byte data
        nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python
    }

    void OnApplicationQuit()
    {
        listener.Stop();
    }
}
