using MixedReality.Toolkit.SpatialManipulation;
using RosMessageTypes.Geometry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Marker_place : MonoBehaviour
{
    public GameObject target;

    public GameObject selection_target;

    public GameObject marker;

    public GameObject parent;

    public List<Vector3> LocalPosList;

    int place = 0;

    int placed = 0;

    int num = 0;

    // Start is called before the first frame update
    void Start()
    {
        LocalPosList = new List<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        place = parent.GetComponent<Place_start>().g_place;
        num = parent.GetComponent<Place_start>().g_num;
        if (place != 0)
        {
            var selected = selection_target.GetComponent<ObjectManipulator>().is_selected;
            var localPos = selection_target.GetComponent<ObjectManipulator>().objectLocalAttachPoint;
            if (selected == 0)
            {
                placed = 0;
            }
            if (selected != 0 && placed == 0)
            {
                Vector3 scaledPos = Vector3.Scale(localPos, target.transform.localScale);
                Vector3 worldPos = target.transform.localRotation * scaledPos + target.transform.localPosition;
                place_marker(worldPos);
                LocalPosList.Add(localPos);
                parent.GetComponent<Place_start>().g_num += 1;
                num = parent.GetComponent<Place_start>().g_num;
                placed = 1;
            }
        }
        else
        {
            LocalPosList.Clear();
        }
    }

    void place_marker(Vector3 marker_pos)
    {
        GameObject point = Instantiate(marker, marker_pos, Quaternion.identity) as GameObject;
        point.name = num.ToString();
        point.GetComponent<Renderer>().material.color = Color.yellow;
        point.transform.parent = parent.transform;
    }

    public void ServiceStart()
    {
        Transform child;
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            child = parent.transform.Find(i.ToString());
            GameObject.Destroy(child.gameObject);
        }
        if (place == 0)
        {
            place = 1;
        }
        else
        {
            place = 0;
        }
        num = 0;
    }
}
