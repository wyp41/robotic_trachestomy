using UnityEngine;

public class IsCollide : MonoBehaviour
{
    Color init_color;
    void OnTriggerEnter(Collider other)//触发器可以穿透，所以把重力去掉  
    {
        print("collided");
        init_color = this.GetComponent<MeshRenderer>().material.color;
        this.GetComponent<MeshRenderer>().material.color = Color.red;
    }
    void OnTriggerExit(Collider other)      //  触发结束被调用  
    {
        print("collide ended");
        this.GetComponent<MeshRenderer>().material.color = init_color;
    }
}