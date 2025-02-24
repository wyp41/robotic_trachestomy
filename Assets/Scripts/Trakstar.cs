using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using System;

public class Trakstar : MonoBehaviour
{
    ROSConnection m_Ros;

    public Vector3 em_pos;
    public Transform probe_tip;
    public Transform EM_end;
    public Transform from_list;
    public Transform to_list;
    public GameObject marker;

    int num = 0;

    public Vector3 unity_pos;

    // Start is called before the first frame update
    void Start()
    {
        m_Ros = ROSConnection.GetOrCreateInstance();
        em_pos = new Vector3(0, 0, 0);
        unity_pos = new Vector3(0, 0, 0);
        num = 0;
        ROSConnection.GetOrCreateInstance().Subscribe<TransformMsg>("matlab_sensor1_msg", em_read);
    }

    void em_read(TransformMsg msg)
    {
        em_pos.x = (float) msg.translation.x;
        em_pos.y = (float) msg.translation.y;
        em_pos.z = -(float) msg.translation.z;
    }

    public void place_corr()
    {
        GameObject from = Instantiate(marker, em_pos, Quaternion.identity) as GameObject;
        from.name = num.ToString();
        from.GetComponent<Renderer>().material.color = Color.yellow;
        from.transform.parent = from_list;
        GameObject to = Instantiate(marker, unity_pos, Quaternion.identity) as GameObject;
        to.name = num.ToString();
        to.GetComponent<Renderer>().material.color = Color.yellow;
        to.transform.parent = to_list;
        num ++;
    }

    // Update is called once per frame
    void Update()
    {
        unity_pos = probe_tip.position;
        EM_end.localPosition = em_pos;
    }
}
