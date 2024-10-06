using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] BoxCollider2D moveCollider;
    float speed;
    public Vector2 velocity;
    
    private bool active;

    int boundariesMask;
    private RaycastHit2D hit;

    private void Awake()
    {
        boundariesMask = LayerMask.GetMask("Bounds");
    }

    public void Init(float moveSpeed, bool activate)
    {
        speed = moveSpeed;
        if (activate)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    public void Activate()
    {
        active = true;
        velocity = Vector2.zero;
    }

    public void Deactivate()
    {
        active = true;
        velocity = Vector2.zero;
    }

    public void UpdateMove(float dt, float axis)
    {
        if (!active)
        {
            return;
        }

        Vector2 pos = transform.position;
        var boxSize = moveCollider.size;

        if(Mathf.Approximately(axis, 0f))
        {
            return;
        }

        var vNorm = axis > 0 ? Vector2.right : Vector2.left;

        Vector2 boxPos = (Vector2)moveCollider.transform.position + moveCollider.offset;
        Vector2 delta = boxPos - pos;

        hit = Physics2D.BoxCast(boxPos, moveCollider.size, 0f, vNorm,
            speed * dt, boundariesMask);
        if (hit.collider != null)
        {
            boxPos = hit.centroid - vNorm * 0.02f;
            pos = boxPos - delta;
            velocity = speed * Mathf.Abs(axis) * vNorm;
        }
        else
        {
            velocity = axis * speed * Vector2.right;
            pos += velocity * dt;
        }
        transform.position = pos;
    }
}
