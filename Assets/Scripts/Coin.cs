using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D otherCollider){
        if (otherCollider.tag == "John"){
            InstantiateAtMouseClick.instance.AddItemToQueue();
            Destroy(gameObject);
        }
    }
}
