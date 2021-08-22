using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public float speed;
    private RectTransform _rectTransfiorm;

    void Start(){
        _rectTransfiorm = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = _rectTransfiorm.position;
        _rectTransfiorm.position = new Vector3(pos.x - speed * Time.deltaTime, pos.y, pos.z);
    }
}
