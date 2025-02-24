using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class Manipulator : MonoBehaviour
{
    public Transform end_effector;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        this.transform.position = end_effector.position;
        this.transform.rotation = end_effector.rotation;
    }
}
