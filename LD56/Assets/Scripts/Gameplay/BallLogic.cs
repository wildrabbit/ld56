using System;
using UnityEngine;


public class BallLogic : MonoBehaviour
{
    public int tier;
    [SerializeField] BallLogic splitPrefab;
    
    [SerializeField] float radius = 1f;
    [SerializeField] float startAngleDegrees = 30f;
    [SerializeField] float maxHeight;
    [SerializeField] float maxLength;
    [SerializeField] float accel = -9.8f;
    [SerializeField] float speed;
    [SerializeField] bool startActive;
    [SerializeField] GameObject bichiInsideVFX;

    [SerializeField] Rect bounds = new Rect(-10f, -5.5f, 20, 11);

    public int NumSplitBallLeaves => (int)Mathf.Pow(2, tier - 1);

    public Action<BallLogic, BallLogic[]> Split;
    public Action<BallLogic, bool> Popped;
    public Action<BallLogic> Destroyed;

    bool active = false;
    Collider2D collisionShape;

    public bool bichiInside = false;

    Vector2 acceleration;
    Vector2 velocity;

    RaycastHit2D bounceRaycastResult;
    int bounceLayer;
    private bool flip;
    
    private void Awake()
    {
        collisionShape = GetComponentInChildren<Collider2D>();
                
        acceleration = accel * Vector2.up;
        velocity = Vector2.zero;
        bounceLayer = LayerMask.GetMask("Bounds", "Destructible");
        if (bichiInsideVFX != null)
        {
            bichiInsideVFX.SetActive(bichiInside);
        }

        if(startActive)
        {
            Activate();
        }
    }

    // Differentiate cases?
    public void Stop()
    {
        active = false;
    }

    public void Init(float radius, float maxHeight, float accel, float speed, bool activate = true)
    {
        this.radius = radius;
        this.maxHeight = maxHeight;
        this.accel = accel;
        this.speed = speed;

        acceleration = Vector2.up * accel;

        if (activate)
        {
            Activate();
        }
        else
        {
            velocity = Vector2.zero;
        }
    }

    public void Activate()
    {
        var startVY = speed * Mathf.Sin(startAngleDegrees * Mathf.Deg2Rad);
        var twog = -accel * 2;
        float maxHeight = (startVY * startVY + twog * transform.position.y) / twog;

        velocity = Vector2.zero;
        velocity = Vector2Utils.FromLengthAngle(speed, startAngleDegrees);
        if(flip)
        {
            velocity.x = -velocity.x;
        }
        active = true;
    }

    public void Pop(bool playerHit = false)
    {
        Popped?.Invoke(this, !playerHit);
        if(splitPrefab != null)
        {
            SplitInto();
        }
        //else if(bichiPrefab != null && spawnBichi && !playerHit)
        //{
        //    SpawnBichi(bichiPrefab);
        //}
        //else
        
        {
            Kill();
        }
    }



    public void Kill(bool notify = true)
    {
        if(notify)
        {
            Destroyed?.Invoke(this);
        }        
        Destroy(gameObject);
    }

    public void OnDestroy()
    {
        Destroyed?.Invoke(this);
    }

    public void SplitInto()
    {
        Vector2 rootPos = transform.position;
        // FIND POSITIONS!
        var left = Instantiate(splitPrefab, transform.parent);
        var pos = rootPos + Vector2.left * (left.radius + 0.5f);
        if(Physics2D.OverlapCircle(pos, left.radius, bounceLayer))
        {
            pos = rootPos;
        }
        left.transform.position = pos;
        left.flip = true;

        // right:
        var right = Instantiate(splitPrefab, null);
        pos = rootPos + Vector2.right * (right.radius + 0.5f);
        if (Physics2D.OverlapCircle(pos, right.radius, bounceLayer))
        {
            pos = rootPos;
        }
        right.transform.position = pos;

        Split?.Invoke(this, new BallLogic[] { left, right });
    }

    public void Update()
    {
        if (!active) return;

        float delta = Time.deltaTime;
        UpdateMovement(delta);
    }

    private void UpdateMovement(float deltaTime)
    {
        Vector2 vNorm = velocity.normalized;
        float vMag = velocity.magnitude;
        Vector2 pos = transform.position;
        bounceRaycastResult = Physics2D.CircleCast(pos, radius, vNorm, velocity.magnitude * deltaTime, bounceLayer);

        if(bounceRaycastResult.collider == null)
        {
            pos.x += velocity.x * deltaTime;
            pos.y += velocity.y * deltaTime;
            velocity.x += acceleration.x * deltaTime;
            velocity.y += 0.5f * acceleration.y * deltaTime;
        }
        else
        {
            pos = bounceRaycastResult.centroid - vNorm * 0.02f;
            velocity = vMag * Vector2.Reflect(vNorm, bounceRaycastResult.normal);
        }

        transform.position = pos;
    }

    internal void SetBichiInside(bool v)
    {
        bichiInside = v;
        if (bichiInsideVFX != null)
        {
            bichiInsideVFX.SetActive(v);
        }
    }
}
