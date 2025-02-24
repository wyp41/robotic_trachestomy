using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

//https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
public class SmoothCurveHandler : MonoBehaviour
{
    Thread mThread;

    public GameObject local;

    public string connectionIP = "127.0.0.1";
    public int connectionPort = 25001;

    public Transform parent;
    public Transform select_ROI;
    public Transform ur5;

    public List<Vector3> target;
    public Vector3 ee_pos;

    public Transform waypoint;

    public LineRenderer lineRenderer1;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;
    bool running;
    bool stopped;
 
    const int countBetween2Point = 20;
 
    List<Vector3> curvePoints;

    List<Vector3> LocalPosList;

    List<Vector3> posList;

    List<Vector3> waypointList;
 
    void Start()
    {
        posList = new List<Vector3>();
        curvePoints = new List<Vector3>();
        LocalPosList = new List<Vector3>();
        target = new List<Vector3>();
        waypointList = new List<Vector3>();
        //waypointList.Add(new Vector3(-0.09008026f, 0.09045438f, 0.615966f));
        //waypointList.Add(new Vector3(1,1,1));
        //LocalPosList = local.GetComponent<Marker_place>().LocalPosList;
        //LocalPosList.Add(new Vector3(-0.5f, 0.5f, 0));
        //LocalPosList.Add(new Vector3(0.5f, 0.5f, 0));
        //LocalPosList.Add(new Vector3(0.5f, -0.5f, 0));
        //LocalPosList.Add(new Vector3(-0.5f, -0.5f, 0));
        //ThreadStart ts = new ThreadStart(GetInfo);
        //mThread = new Thread(ts);
        //mThread.Start();
    }

    void Update()
    {
        // Transform from local pose to world pose
        posList.Clear();
        curvePoints.Clear();
        target.Clear();

        for (int i = 0; i < waypointList.Count; i++)
        {
            //Vector3 scaledPos = Vector3.Scale(ee_pos, parent.localScale);
            //Vector3 worldPos = waypointList[i] - parent.localRotation * scaledPos;
            Vector3 worldPos = waypointList[i];
            target.Add(Quaternion.Inverse(ur5.transform.rotation) * worldPos - Quaternion.Inverse(ur5.transform.rotation) * ur5.transform.position);
        }
        //LocalPosList = local.GetComponent<Marker_place>().LocalPosList;

        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = curvePoints.Count;
        lineRenderer1.positionCount = curvePoints.Count;
        for (int i = 0; i < LocalPosList.Count; i++){
            Vector3 scaledPos = Vector3.Scale(LocalPosList[i], select_ROI.localScale);
            Vector3 worldPos = select_ROI.localRotation * scaledPos + select_ROI.localPosition;
            posList.Add(worldPos);
        }

        if (posList.Count > 3){
            CalculateCurve();
            // LineRenderer lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = curvePoints.Count;
            lineRenderer1.positionCount = curvePoints.Count;
            for (int i = 0; i < curvePoints.Count; i++)
            {
                Vector3 scaledPos = Vector3.Scale(curvePoints[i], parent.localScale);
                Vector3 worldPos = parent.localRotation * scaledPos + parent.localPosition;
                //scaledPos = Vector3.Scale(curvePoints[i]-ee_pos, parent.localScale);
                //Vector3 eePos = parent.localRotation * scaledPos + parent.localPosition;
                //Vector3 worldPos = parent.transform.rotation * scaledPos + parent.transform.position;
                //target.Add(Quaternion.Inverse(ur5.transform.rotation)*eePos - Quaternion.Inverse(ur5.transform.rotation)*ur5.transform.position);
                target.Add(Quaternion.Inverse(ur5.transform.rotation) * worldPos - Quaternion.Inverse(ur5.transform.rotation) * ur5.transform.position);
                lineRenderer.SetPosition(i, worldPos);
            }

            for (int i = 0; i < curvePoints.Count; i++)
            {
                Vector3 scaledPos = Vector3.Scale(curvePoints[i], select_ROI.localScale);
                Vector3 worldPos = select_ROI.localRotation * scaledPos + select_ROI.localPosition;
                lineRenderer1.SetPosition(i, worldPos);
            }
        }

        //Transform transform;
        //for(int i = 0;i < this.transform.childCount; i++)
        //{
        //    transform = this.transform.GetChild(i);
        //    GameObject.Destroy(transform.gameObject);
        //}

        //for (int i = 0; i < posList.Count; i++){
        //    var marker=GameObject.CreatePrimitive(PrimitiveType.Sphere);//类型
        //    marker.name="marker"+i.ToString();
        //    marker.GetComponent<Renderer>().material.color=Color.yellow;//颜色
        //    marker.transform.parent = this.transform;
        //    marker.transform.localScale = new Vector3 (0.008f, 0.008f, 0.008f);
        //    marker.transform.position=posList[i];
        //}
        // print(curvePoints);
    }

    public void Add_waypoints()
    {
        waypointList.Add(waypoint.position);
        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);//类型
        marker.name = waypointList.Count.ToString();
        marker.GetComponent<Renderer>().material.color = Color.yellow;//颜色
        marker.transform.parent = this.transform;
        marker.transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);
        marker.transform.position = waypoint.position;
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
        print(dataReceived);
        if (dataReceived != null)
        {
            if (dataReceived == "r"){
                LocalPosList.Clear();
            }
            else{
                //---Using received data---
                Vector3 markerPos = StringToVector3(dataReceived);
                if (!LocalPosList.Contains(markerPos)){
                    LocalPosList.Add(markerPos);
                }
            }

            //---Sending Data to Host----
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("received"); //Converting string to byte data
            nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python
        }
    }
 
    //-----------------------

    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        sVector = sVector.Replace("(", "");
        sVector = sVector.Replace(")", "");

        // split the items
        string[] sArray = sVector.Split(',');

        Vector3 VArray;

        VArray.x = float.Parse(sArray[0]);
        VArray.y = float.Parse(sArray[1]);
        VArray.z = float.Parse(sArray[2]);

        // store as a Vector3
        //Vector3 result = new Vector3(
        //    float.Parse(sArray[0]),
        //    float.Parse(sArray[1]),
        //    float.Parse(sArray[2]));
        return VArray;
    }

    public Vector2 ToMyVector2(Vector3 v3)
    {
        // return new Vector2(v3.x, v3.z);// Discarding the y instead of z captain.
        return new Vector2(v3.x, v3.y);// Discarding the y instead of z captain.
    }

    public Vector3 ToMyVector3(Vector2 v2)
    {
        // return new Vector3(v2.x, 1, v2.y);// set y to 0.
        return new Vector3(v2.x, v2.y, 0);// set y to 0.
    }
 
    Vector3 firstPos, curPos, nextPos, lastPos;
    void CalculateCurve()
    {
        //依次计算相邻两点间曲线
        //由四个点确定一条曲线（当前相邻两点p1,p2，以及前后各一点p0,p3）
        for (int i = 0; i < LocalPosList.Count; i++)
        {
            //特殊位置增加虚拟点
            //如果p1点是第一个点，不存在p0点，由p1,p2确定一条直线，在向量(p2p1)方向确定虚拟点p0
            // if (i == 0)
            //     firstPos = LocalPosList[i] * 2 - LocalPosList[i + 1];
            // else
            //     firstPos = LocalPosList[i - 1];
            int n = LocalPosList.Count;
            firstPos = LocalPosList[(n + i - 1) % n];
            //中间点
            curPos = LocalPosList[i % n];
            nextPos = LocalPosList[(i + 1) % n];
            //特殊位置增加虚拟点，同上
            // if (i == LocalPosList.Count - 2)
            //     lastPos = LocalPosList[i + 1] * 2 - LocalPosList[i];
            // else
            //     lastPos = LocalPosList[i + 2];
            lastPos = LocalPosList[(i + 2) % n];
 
            CatmulRom(ToMyVector2(firstPos), ToMyVector2(curPos), ToMyVector2(nextPos), ToMyVector2(lastPos), ref curvePoints, countBetween2Point * i);
        }
        //加入最后一个点位
        // curvePoints.Add(LocalPosList[LocalPosList.Count - 1]);
    }
 
    //平滑过渡两点间曲线（p1,p2为端点，p0,p3是控制点）
    void CatmulRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ref List<Vector3> points, int startIndex)
    {
        //计算Catmull-Rom样条曲线
        float t0 = 0;
        float t1 = GetT(t0, p0, p1);
        float t2 = GetT(t1, p1, p2);
        float t3 = GetT(t2, p2, p3);
 
        float t;
        for (int i = 0; i < countBetween2Point; i++)
        {
            t = t1 + (t2 - t1) / countBetween2Point * i;
 
            Vector2 A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
            Vector2 A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
            Vector2 A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;
 
            Vector2 B1 = (t2 - t) / (t2 - t0) * A1 + (t - t0) / (t2 - t0) * A2;
            Vector2 B2 = (t3 - t) / (t3 - t1) * A2 + (t - t1) / (t3 - t1) * A3;
 
            Vector2 C = (t2 - t) / (t2 - t1) * B1 + (t - t1) / (t2 - t1) * B2;
 
            points.Add(ToMyVector3(C));
        }
    }
 
    float GetT(float t, Vector2 p0, Vector2 p1)
    {
        return t + Mathf.Pow(Mathf.Pow((p1.x - p0.x), 2) + Mathf.Pow((p1.y - p0.y), 2), 0.5f);
    }
 
}