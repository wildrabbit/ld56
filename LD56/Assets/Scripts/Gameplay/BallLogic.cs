using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BallLogic : MonoBehaviour
{
    [SerializeField] BallLogic splitPrefab;
    // [SerializeField] Bichi bichiPrefab;

    [SerializeField] float radius = 1f;
    [SerializeField] float startAngleDegrees = 30f;
    [SerializeField] float maxHeight;
    [SerializeField] float maxLength;
    [SerializeField] float accel = -9.8f;
    [SerializeField] float speed;
    [SerializeField] bool startActive;

    [SerializeField] Rect bounds = new Rect(-10f, -5.5f, 20, 11);

    bool active = false;
    Collider2D collisionShape;

    // [SerializeField] bool popBichi = false;
    // [SerializeField] GameObject bichiPrefab;

    Vector2 acceleration;
    Vector2 velocity;

    RaycastHit2D bounceRaycastResult;
    int bounceLayer;

    private void Awake()
    {
        collisionShape = GetComponentInChildren<Collider2D>();
        
        acceleration = accel * Vector2.up;
        velocity = Vector2.zero;
        bounceLayer = LayerMask.GetMask("Bounds");

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
        active = true;
    }

    public void Pop()
    {
        if(splitPrefab != null)
        {
            Split();
        }
        // else: Bichi stuff
        Destroy(gameObject);
    }

    public void Split()
    {
        Vector2 rootPos = transform.position;
        // FIND POSITIONS!
        var left = Instantiate(splitPrefab, null);
        var pos = rootPos + Vector2.left * (left.radius + 0.5f);
        left.transform.position = pos;

        // right:
        var right = Instantiate(splitPrefab, null);
        pos = rootPos + Vector2.right * (right.radius + 0.5f);
        right.transform.position = pos;
    }

    public void Update()
    {
        if (Keyboard.current.zKey.isPressed)
        {
            Pop();
            return;
        }

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
}
