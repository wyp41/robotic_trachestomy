using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class model_switch : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject model1;
    public GameObject model2;
    int cur_model = 0;
    void Start()
    {
        cur_model = 0;
    }

    public void switch_model()
    {
        if (cur_model == 0)
        {
            cur_model = 1;
            model1.SetActive(false); 
            model2.SetActive(true);
        }
        else
        {
            cur_model = 0;
            model1.SetActive(true);
            model2.SetActive(false);
        }
    }
}
