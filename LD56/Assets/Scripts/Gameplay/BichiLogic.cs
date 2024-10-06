using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BichiLogic : MonoBehaviour
{
    [SerializeField] bool startActive;
    [SerializeField] float dropSpeed = 3f; // It won't use gravity!
    [SerializeField] float driftLimit = 0.2f; //Horz axis
    [SerializeField] float driftFreq = 3f; //Horz axis

    bool active = false;
    Vector2 velocity;

    Collider2D hitCollider;
    ContactFilter2D boundsFilter;
    List<Collider2D> boundsHit = new List<Collider2D>();
    Vector2 startPos;
    float startNoise = 0f;

    private void Awake()
    {
        active = startActive;
        hitCollider = GetComponentInChildren<Collider2D>();
        startPos = transform.position;
        boundsFilter = new ContactFilter2D()
        {
            useLayerMask = true,
            layerMask = LayerMask.GetMask("Bounds")
        };
    }

    public void StartGame()
    {
        Activate();
    }

    public void Activate()
    {
        active = true;
        startNoise = UnityEngine.Random.Range(0, 360);
        velocity = Vector2.zero;
        boundsHit.Clear();
    }

    public void Deactivate()
    {
        active = false;
        velocity = Vector2.zero;
    }
    public void Kill()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        if(!active)
        {
            return;
        }

        var pos = transform.position;
        pos.x = startPos.x + driftLimit * Mathf.Sin(startNoise + driftFreq * Time.time);
        pos.y = pos.y -dropSpeed * Time.deltaTime;
        transform.position = pos;
        

        int numHits = Physics2D.OverlapCollider(hitCollider, boundsFilter, boundsHit);
        if(numHits > 0) 
        {
            Kill();
        }
    }
}
