using UnityEngine;
using UnityEngine.UI;

public class Virtual_camera : MonoBehaviour
{
    public Camera secondaryCamera; // 拖入新建的 Camera
    public RawImage rawImage;      // 拖入场景中的 RawImage

    public float fx = 259.48433f;  // 像素焦距 (X 方向)
    public float fy = 259.54973f;  // 像素焦距 (Y 方向)
    public float cx = 206.90763f;   // 主点横坐标 (像素)
    public float cy = 212.13226f;   // 主点纵坐标 (像素)
    //public float cx = 200f;   // 主点横坐标 (像素)
    //public float cy = 200f;   // 主点纵坐标 (像素)

    // 图像分辨率
    public int imageWidth = 400;  // 图像宽度
    public int imageHeight = 400; // 图像高度

    void Start()
    {
        // 创建 Render Texture
        RenderTexture renderTexture = new RenderTexture(400, 400, 24);
        renderTexture.Create();

        // 将 Render Texture 赋值给 Camera 和 RawImage
        secondaryCamera.nearClipPlane = 0.0001f;

        float near = secondaryCamera.nearClipPlane;
        float far = secondaryCamera.farClipPlane;

        Matrix4x4 projectionMatrix = Matrix4x4.zero;

        projectionMatrix[0, 0] = 2.0f * fx / imageWidth; // (0, 0)
        projectionMatrix[0, 2] = 1.0f - 2.0f * cx / imageWidth; // (0, 2)
        projectionMatrix[1, 1] = 2.0f * fy / imageHeight; // (1, 1)
        projectionMatrix[1, 2] = 2.0f * cy / imageHeight - 1.0f; // (1, 2)
        projectionMatrix[2, 2] = -(far + near) / (far - near); // (2, 2)
        projectionMatrix[2, 3] = -(2.0f * far * near) / (far - near); // (2, 3)
        projectionMatrix[3, 2] = -1.0f; // (3, 2)

        secondaryCamera.projectionMatrix = projectionMatrix;

        secondaryCamera.targetTexture = renderTexture;
        rawImage.texture = renderTexture;
    }
}