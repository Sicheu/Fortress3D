using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.gameObject.tag == "Shell")
    //     {
    //         Destroy(gameObject);
    //     }
    // }

    public void Exploded()
    {
        Destroy(gameObject);
    }
}
