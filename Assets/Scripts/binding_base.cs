using System.Collections;
using System.Collections.Generic;
using Unity.Loading;
using UnityEngine;

public class binding_base : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform base2unity;
    public Transform model2unity;
    public Transform model2base;
    public Transform localPose;

    bool isBinding = false;
    bool isLoaded = false;
    void Start()
    {
        isBinding = false;
        isLoaded = false;
    }

    public void loading()
    {
        model2base.position = new Vector3(-0.05282751f, 0.02163661f, -0.05932391f);
        model2base.rotation = new Quaternion(-0.00878f, -0.02796f, -0.06076f, -0.99772f);
        isLoaded = !isLoaded;
    }

    public void binding()
    {

        Quaternion baseRotationInverse = Quaternion.Inverse(base2unity.rotation); // 逆旋转
        Vector3 basePositionInverse = -(baseRotationInverse * base2unity.position); // 逆平移
        Quaternion model2baseRotation = model2unity.rotation * baseRotationInverse;

        // 计算 model2base 的位置
        Vector3 model2basePosition = baseRotationInverse * (model2unity.position - base2unity.position);
        model2base.position = model2basePosition;
        model2base.rotation = model2baseRotation;

        isBinding = !isBinding;
    }

    // Update is called once per frame
    void Update()
    {
        if (isBinding | isLoaded)
        {
            // 计算 model2unity 的位置
            //Vector3 model2unityPosition = base2unity.TransformPoint(model2base.position);

            //// 计算 model2unity 的旋转
            //Quaternion model2unityRotation = model2base.rotation * base2unity.rotation;
            //model2unity.position = model2unityPosition;
            //model2unity.rotation = model2unityRotation;
            localPose.localPosition = model2base.position;
            localPose.localRotation = model2base.rotation;
            model2unity.position = localPose.position;
            model2unity.rotation = localPose.rotation;
        }
    }
}
