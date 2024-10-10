using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BichiLogic : MonoBehaviour
{
    const string ceiling = "ceiling";
    const string ground = "ground";

    [SerializeField] bool startActive;
    [SerializeField] float timeLimit = -1;

    [SerializeField] float launchSpeed = 10f;
    [SerializeField] float dropSpeed = 3f; // It won't use gravity!
    [SerializeField] float driftLimit = 0.2f; //Horz axis
    [SerializeField] float driftFreq = 3f; //Horz axis

    public event Action<BichiLogic> Destroyed;
    public event Action<BichiLogic> Dead;

    bool active = false;
    public bool flying = false;
    Vector2 velocity;

    float elapsed;

    CircleCollider2D hitCollider;
    ContactFilter2D boundsFilter;
    List<Collider2D> boundsHit = new List<Collider2D>();
    Vector2 startPos;
    float startNoise = 0f;
    LayerMask boundsMask;
    LayerMask destructibleMask;

    Vector3 defaultScale;

    private void Awake()
    {
        active = startActive;
        hitCollider = GetComponentInChildren<CircleCollider2D>();
        boundsMask = LayerMask.GetMask("Bounds");
        destructibleMask = LayerMask.GetMask("Destructible");
        defaultScale = transform.localScale;
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
        hitCollider.enabled = true;
        if (timeLimit > 0f)
        {
            elapsed = 0f;
        }
        else
        {
            elapsed = -1f;
        }
        boundsHit.Clear();
    }

    public void Deactivate()
    {
        active = false;
        hitCollider.enabled = false;
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

        if(elapsed >= 0f)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= timeLimit)
            {
                Die();
            }
        }

        Vector2 pos = transform.position;
        Vector2 hitColliderPos = (Vector2)hitCollider.transform.position + hitCollider.offset;
        Vector2 delta = hitColliderPos - pos;

        pos.x = startPos.x + driftLimit * Mathf.Sin(startNoise + driftFreq * Time.time);
        pos.y = pos.y -dropSpeed * Time.deltaTime;

        var colliding = Physics2D.OverlapCircle(pos + delta, hitCollider.radius, destructibleMask);
        if(colliding == null)
        {
            transform.position = pos;
        }

        var boundsHit = Physics2D.OverlapCircleAll(hitColliderPos, hitCollider.radius, boundsMask);
        if (boundsHit.Length == 0 || (boundsHit.Length == 1 && boundsHit[0].gameObject.CompareTag(ceiling)))
        {
            return;
        }
        else if(Array.FindIndex(boundsHit, 0, boundsHit.Length, x => x.gameObject.CompareTag(ground)) >= 0)
        {
            Die();
        }
        else
        {
            transform.position = pos;
        }
    }

    public void Die()
    {
        Dead?.Invoke(this);
        // Animate
        Kill();
    }

    internal void LaunchSequence(Vector3 ballPosition, Vector3 flyTargetReference)
    {
        var pos = ballPosition;
        transform.position = pos;
        Deactivate();
        transform.localScale = defaultScale * 0.5f;
        float flyHeight = flyTargetReference.y;
        float delta = flyHeight - pos.y;
        float duration = delta / launchSpeed;
        flying = true;
        transform.DOMoveY(flyHeight, duration).SetEase(Ease.OutQuart).OnComplete(OnLaunchSequenceCompleted);
    }

    private void OnLaunchSequenceCompleted()
    {
        transform.localScale = defaultScale;
        flying = false;
        Activate();
    }

    private void OnDestroy()
    {
        int tweensKilled = DOTween.Kill(this.transform);
        Debug.Log($"Destroy bichi - {tweensKilled} tweens killed");
    }
}
