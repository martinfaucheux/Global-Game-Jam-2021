using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public List<prob> spawns = new List<prob>();
    public bool waitForPlayerDistance = false;
    public float distanceFromPlayerLoad = 100f;
    private bool stop = false;
    [Serializable]
    public class prob
    {
        [Range(0, 1)] public float weight = 1f;
        public GameObject prefab;
    }

    // Start is called before the first frame update
    void Awake()
    {
        stop = false;
        if (waitForPlayerDistance)
            StartCoroutine(Wait(2f));
        else
            pickAndSpawn();
    }

    private void pickAndSpawn()
    {
        float totalweight = 0f;
        foreach (prob p in spawns)
        {
            totalweight += p.weight;
        }
        float RandomVal = UnityEngine.Random.Range(0f, 1f);

        foreach (prob p in spawns)
        {
            float weightedAvg = p.weight / totalweight;
            if (RandomVal < weightedAvg)
            {
                spawn(p.prefab);
                return;
            }
            else
                RandomVal -= weightedAvg;
        }
    }

    private void OnDestroy()
    {
        stop = true;
    }

    private void spawn(GameObject prefab)
    {
        Instantiate(prefab, this.transform);
    }

    IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        

        if (GameManager.instance.playerPosition != Vector3.positiveInfinity 
            && Mathf.Abs(GameManager.instance.playerPosition.x - this.transform.position.x) < distanceFromPlayerLoad)
        {
                pickAndSpawn();
        }
        else
        {
            if(!stop)
                    StartCoroutine(Wait(time));
        }
        
    }
}
