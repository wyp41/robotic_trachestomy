using UnityEngine;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Pcx;

public class HttpReader : MonoBehaviour
{
    private HttpClient client;
    int num = 1;
    int started = 0;
    bool reading = false;
    PointCloudRenderer pclRenderer;
    PlyImporterRuntime importer;
    //MeshCollider meshCollider;
    public string filePath;

    void Start()
    {
        num = 1;
        //filePath = Application.dataPath + "/test.ply";
        //meshCollider = GetComponent<MeshCollider>();
        importer = new PlyImporterRuntime();
        client = new HttpClient();
        StartCoroutine(ReadDataLoop());
    }

    private void Update()
    {
        if (reading)
        {
            reading = false;
            var readPath = Application.persistentDataPath + "/" + (started).ToString() + ".ply";
            //print(readPath);
            Mesh mesh = importer.ImportAsMesh(readPath);
            var meshfilter = this.GetComponent<MeshFilter>().mesh = mesh;
            //mesh.RecalculateNormals();
            //meshCollider.sharedMesh = mesh;
        }
    }

    private IEnumerator ReadDataLoop()
    {
        while (true)
        {

            yield return ReadData();
            yield return new WaitForSeconds(0.02f);
        }
    }

    private IEnumerator ReadData()
    {
        string url = "http://192.168.0.149:5000/uploads/" + num.ToString() + ".ply";
        //print(url);

        Task<byte[]> downloadTask = client.GetByteArrayAsync(url);
        yield return new WaitUntil(() => downloadTask.IsCompleted);

        if (downloadTask.Exception == null)
        {
            byte[] fileData = downloadTask.Result;
            filePath = Application.persistentDataPath + "/" + num.ToString() + ".ply";
            //print(filePath);
            File.WriteAllBytes(filePath, fileData);
            num++;
            started++;
            if (num > 10)
            {
                num = 1;
            }
            if (started > 10)
            {
                started = 1;
            }
            //reading = true;
            //print(readPath);
            Mesh mesh = importer.ImportAsMesh(filePath);
            var meshfilter = this.GetComponent<MeshFilter>().mesh = mesh;

            //yield return new WaitForSeconds(0.01f);

            //print(filePath);
            //print("num:" + num.ToString());
            //print("started:" + started.ToString());
            //Debug.Log("finished with len" + fileData.Length);
        }
        else
        {
            Debug.Log("failed" + downloadTask.Exception.Message);
        }
    }

    void OnDestroy()
    {
        client.Dispose();
    }
}