using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class FollowMouse : MonoBehaviour
{
    Rigidbody2D rigidbody2d;
    private Vector3 newPosition;
    public float precision_x;
    public float precision_y;
    public bool pair_x, pair_y;

    public bool can_drop = true;
    private bool is_dropped = false;

    private Vector3 hitPos;
    private ParticleSystem[] _particleComponents;

    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        rigidbody2d = GetComponent<Rigidbody2D>();
        _particleComponents = GetComponentsInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_dropped)
        {
            newPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            newPosition.z = 0;
            float preview_x = roundPlacement(newPosition.x, precision_x, pair_x);
            float preview_y = roundPlacement(newPosition.y, precision_y, pair_y);

            Vector2 newPos = new Vector2(preview_x, preview_y);
            transform.position = newPos;
        }

        if (
            InstantiateAtMouseClick.instance.canClick
            & Input.GetButtonDown("Fire1") && can_drop)
        {
            drop();
        }
    }


    void FixedUpdate()
    {
        List<Collider2D> colliders;
        colliders = GetComponentsInChildren<Collider2D>().ToList();
        List<bool> hits = new List<bool>();

        foreach (Collider2D coll in colliders)
        {
            // Cast a ray straight down.
            Vector3 pos = new Vector3();
            pos = coll.gameObject.transform.position;
            //pos = coll.bounds.center;
            //coll.bounds.extents.
            float ray_x = pos.x;
            float ray_y = offset(pos.y, precision_y); // round(pos.y, precision_y, false);
            Vector3 rayVector = new Vector3(ray_x, ray_y, 0);
            RaycastHit2D hit = Physics2D.Raycast(rayVector, Vector3.forward);

            // If it hits something...
            if (hit.collider != null)
            {
                // Debug.Log("hit name" + coll.gameObject.name);
                hitPos = hit.point;
                hits.Add(true);
            }
            else
            {
                hits.Add(false);
            }
        }
        if (!hits.Contains(true))
        {
            can_drop = true;
            HandleSprites(can_drop);
        }
        else
        {
            can_drop = false;
            HandleSprites(can_drop);
        }

    }

    private void HandleSprites(bool canDrop)
    {
        SpriteHandler[] spriteHandler = GetComponentsInChildren<SpriteHandler>();

        foreach (SpriteHandler handler in spriteHandler)
        {
            if (canDrop)
                handler.OnAllow.Invoke();
            else
                handler.OnBlock.Invoke();
        }
    }

    void drop()
    {

        Camera.main.GetComponent<CameraShake>().ShakeCamera(GameManager.instance.CameraShakePower, 0.002f);
        PlayParticles();

        GameEvents.instance.PutBlockTrigger();
        float drop_x = roundPlacement(transform.position.x, precision_x, pair_x);
        float drop_y = roundPlacement(transform.position.y, precision_y, pair_y);
        Vector2 newPos = new Vector2(drop_x, drop_y);
        Destroy(rigidbody2d);
        transform.position = newPos;
        is_dropped = true;
        // Colliders
        List<Collider2D> colliders = transform.gameObject.GetComponentsInChildren<Collider2D>()?.ToList();
        if (colliders == null)
        {
            Debug.LogError("No colliders on new block");
        }
        else
        {
            colliders.ForEach(x => x.enabled = true);
            GameManager.instance.RegisterBlock(colliders);
        }

        this.enabled = false;
    }

    float roundPlacement(float value, float precision, bool pair)
    {
        float full = (float)Math.Floor(value / precision);
        float offset = pair ? 0f : precision / 2f;

        return full * precision + offset;
    }

    float offset(float value, float precision)
    {
        return value - precision / 2f;
        /*
        float full = (float)Math.Floor(value / precision);
        float offset = pair ? 0f : precision / 2f;

        return full * precision + offset;
        */
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(hitPos, 0.1f);
    }

    private void PlayParticles()
    {
        foreach (ParticleSystem particles in _particleComponents)
        {
            Debug.Log("play particles");
            particles.Play();
        }
    }

}

