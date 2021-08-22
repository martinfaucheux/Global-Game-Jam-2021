using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProbSpawning : MonoBehaviour
{
    [SerializeField] [Range(0, 1)] private float probabilityOfSpawn = 1f;

    void Awake()
    {
        if (probabilityOfSpawn < 0.99f)
        {
            if (Random.Range(0f, 1f) > probabilityOfSpawn) despawn();
        }

    }

    private void despawn()
    {
        this.gameObject.SetActive(false);
    }
}
