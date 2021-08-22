using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SpriteFlipper : MonoBehaviour
{

    SpriteRenderer _spriteRendered;

    AIPath _aIPath;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRendered = GetComponentInChildren<SpriteRenderer>();   
        _aIPath = GetComponent<AIPath>();   
    }

    // Update is called once per frame
    void Update()
    {
        _spriteRendered.flipX = (_aIPath.velocity.x < -0.01f);
    }
}
