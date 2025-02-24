using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debug_module : MonoBehaviour
{
    public GameObject debug;
    public Transform test;
    Text debug_msg;
    // Start is called before the first frame update
    void Start()
    {
        debug_msg = debug.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 copyPosition = new Vector3(test.position.x, test.position.y, test.position.z);
        debug_msg.text = test.rotation.ToString() + "\n" + test.position.x.ToString() + ", " + test.position.y.ToString() + ", " + test.position.z.ToString();
    }
}
