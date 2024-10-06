using System;
using System.Collections.Generic;
using UnityEngine;

public class BichiLogic : MonoBehaviour
{
    const string ceiling = "ceiling";

    [SerializeField] bool startActive;
    
    [SerializeField] float dropSpeed = 3f; // It won't use gravity!
    [SerializeField] float driftLimit = 0.2f; //Horz axis
    [SerializeField] float driftFreq = 3f; //Horz axis


    public event Action<BichiLogic> Destroyed;

    bool active = false;
    Vector2 velocity;

    CircleCollider2D hitCollider;
    ContactFilter2D boundsFilter;
    List<Collider2D> boundsHit = new List<Collider2D>();
    Vector2 startPos;
    float startNoise = 0f;

    private void Awake()
    {
        active = startActive;
        hitCollider = GetComponentInChildren<CircleCollider2D>();
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
        startPos = transform.position;
        velocity = Vector2.zero;
        boundsHit.Clear();
    }

    public void Deactivate()
    {
        active = false;
        velocity = Vector2.zero;
    }
    public void Kill(bool notify = true)
    {
        if (notify)
        {
            Destroyed?.Invoke(this);
        }
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


        Vector2 hitColliderPos = (Vector2)hitCollider.transform.position + hitCollider.offset;
        var boundsHit = Physics2D.OverlapCircleAll(hitColliderPos, hitCollider.radius, LayerMask.GetMask("Bounds"));
        if (boundsHit.Length == 0 || (boundsHit.Length == 1 && boundsHit[0].gameObject.CompareTag(ceiling)))
        {
            return;
        }
        else
        {
            Kill();
        }
    }
}
