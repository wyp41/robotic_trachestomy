using Dummiesman;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using System;

public class ObjFromStream : MonoBehaviour {
    GameObject loadedObj;
    int num = 1;

    Mesh receivedMesh;

    //void Start ()
    //{
    //    receivedMesh = GetComponent<MeshFilter>().mesh;
    //    //print(receivedMesh.vertices);
    //}

    private void Update()
    {
        if (loadedObj != null)
        {
            Destroy(loadedObj);
        }
        if (num > 10)
        {
            num = 1;
        }
        //var www = new WWW("http://192.168.0.149:5000/uploads/" + num.ToString() + ".ply");
        //var www = new WWW("http://192.168.0.149:5000/uploads/1.obj");
        //while (!www.isDone)
        //    System.Threading.Thread.Sleep(1);
        //num++;

        //string path = "G:/AR/AR_robotics" + "/test.obj";
        //File.WriteAllBytes(path, Encoding.UTF8.GetBytes(www.text));

        ////create stream and load
        //var textStream = new MemoryStream(Encoding.UTF8.GetBytes(www.text));
        //loadedObj = new OBJLoader().Load(textStream);
        //loadedObj.transform.SetParent(this.transform);

        
    }
}
