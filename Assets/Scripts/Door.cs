using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D otherCollider){
        if (otherCollider.tag == "John"){
            GameManager.instance.Escape();
        }
    }
}
