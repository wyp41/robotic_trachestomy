using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class Place_start : MonoBehaviour
{
    public GameObject parent;
    public int g_place = 0;
    public int g_num = 0;
    // Start is called before the first frame update
    void Start()
    {
        g_place = 0;
        g_num = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ServiceStart()
    {
        Transform child;
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            child = parent.transform.Find(i.ToString());
            GameObject.Destroy(child.gameObject);
        }
        if (g_place == 0)
        {
            g_place = 1;
        }
        else
        {
            g_place = 0;
        }
        g_num = 0;
    }
}
