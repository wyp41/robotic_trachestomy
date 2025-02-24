using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Camera_read : MonoBehaviour
{
    public RawImage rawImage;//相机渲染的UI
    public GameObject quad;//相机渲染的GameObject
    private WebCamTexture webCamTexture;
    Texture2D mTexture;
    int playing = 0;

    void Start()
    {
        mTexture = new Texture2D(1000, 1000);
        ToOpenCamera();
    }

    /// <summary>
    /// 打开摄像机
    /// </summary>
    public void ToOpenCamera()
    {
        StartCoroutine("OpenCamera");
    }
    public IEnumerator OpenCamera()
    {

        int maxl = Screen.width;
        if (Screen.height > Screen.width)
        {
            maxl = Screen.height;
        }

        // 申请摄像头权限
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
            }

            //打开渲染图
            if (rawImage != null)
            {
                rawImage.gameObject.SetActive(true);
            }
            if (quad != null)
            {
                quad.gameObject.SetActive(true);
            }

            // 监控第一次授权，是否获得到设备（因为很可能第一次授权了，但是获得不到设备，这里这样避免）
            // 多次 都没有获得设备，可能就是真没有摄像头，结束获取 camera
            int i = 0;
            while (WebCamTexture.devices.Length <= 0 && 1 < 300)
            {
                yield return new WaitForEndOfFrame();
                i++;
            }
            WebCamDevice[] devices = WebCamTexture.devices;//获取可用设备

            if (WebCamTexture.devices.Length <= 0)
            {
                Debug.LogError("没有摄像头设备，请检查");
            }
            else
            {
                string devicename = devices[0].name;
                webCamTexture = new WebCamTexture(devicename, maxl, maxl == Screen.height ? Screen.width : Screen.height, 30)
                {
                    wrapMode = TextureWrapMode.Repeat
                };

                webCamTexture.Play();
                playing = 1;
                yield return new WaitForSeconds(0);

                // 渲染到 UI 或者 游戏物体上
                //if (rawImage != null)
                //{
                //    mTexture.SetPixels(webCamTexture.GetPixels(0, 0, 200, 200));
                //    print(webCamTexture.GetPixel(160, 120));
                //    mTexture.Apply();
                //    rawImage.texture = mTexture;
                //}

                //if (quad != null)
                //{
                //    quad.GetComponent<Renderer>().material.mainTexture = webCamTexture;
                //}   
            }

        }
        else
        {
            Debug.LogError("未获得读取摄像头权限");
        }
    }

    private void Update()
    {
        if (playing == 1)
        {
            if (rawImage != null)
            {
                //mTexture.SetPixels(webCamTexture.GetPixels(100, 0, webCamTexture.width, webCamTexture.height));
                //print(webCamTexture.width);
                //print(webCamTexture.height);

                //mTexture.Apply();
                rawImage.texture = webCamTexture;
            }

            if (quad != null)
            {
                quad.GetComponent<Renderer>().material.mainTexture = webCamTexture;
            }
        }
    }

    private void OnApplicationPause(bool pause)
    {
        // 应用暂停的时候暂停camera，继续的时候继续使用
        if (webCamTexture != null)
        {
            if (pause)
            {
                webCamTexture.Pause();
            }
            else
            {
                webCamTexture.Play();
            }
        }

    }


    private void OnDestroy()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
        }
    }
}