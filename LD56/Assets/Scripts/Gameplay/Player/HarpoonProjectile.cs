using System;
using UnityEngine;

public class HarpoonProjectile : MonoBehaviour
{
    [SerializeField] float growSpeed = 2f;
    [SerializeField] float startLength = 0.5f;
    [SerializeField] bool pierceDestructibles = false;
    [SerializeField] Transform refPos;
    [SerializeField] Transform lineRenderer;
    [SerializeField] SpriteRenderer lineSpr;
    [SerializeField] public AudioClip clip;
    
    public AudioSource shootSfx;

    private BoxCollider2D box;
    private PlayerLogic parent;

    LayerMask ballLayer;
    LayerMask destructLayer;
    LayerMask boundsLayer;

    float curLineHeight;

    public event Action<HarpoonProjectile> Destroyed;

    // Use this for initialization
    void Awake()
    {
        box = GetComponentInChildren<BoxCollider2D>();
        ballLayer = LayerMask.GetMask("Balls");
        destructLayer = LayerMask.GetMask("Destructible");
        boundsLayer = LayerMask.GetMask("Bounds");
    }

    public void Init(PlayerLogic parent)
    {
        this.parent = parent;
        RefreshSize(startLength);
        shootSfx.PlayOneShot(clip);
    }

    private void RefreshSize(float len)
    {

        curLineHeight = len;

        if(lineSpr.drawMode == SpriteDrawMode.Tiled)
        {
            var sz = lineSpr.size;
            sz.y = curLineHeight;
            lineSpr.size = sz;
        }
        else
        {
            var lineScale = lineRenderer.localScale;
            lineScale.y = curLineHeight;
            lineRenderer.localScale = lineScale;
        }

        
        var linePos = lineRenderer.localPosition;
        linePos.y = -curLineHeight * 0.5f;
        lineRenderer.localPosition = linePos;

        var tipPos = refPos.localPosition;
        tipPos.y = curLineHeight;
        refPos.localPosition = tipPos;


        var collOffset = box.offset;
        collOffset.y = -curLineHeight * 0.5f;
        box.offset = collOffset;

        var collSize = box.size;
        collSize.y = curLineHeight;
        box.size = collSize;
    }

    // Update is called once per frame
    void Update()
    {
        var boxPos = (Vector2)box.transform.position + box.offset;
        var res = Physics2D.OverlapBoxAll(boxPos, box.size, 0f, ballLayer);
        bool kill = false;
        if (res != null)
        {
            int numPopped = 0;
            foreach(var collider in res)
            {
                var ball = collider.GetComponentInParent<BallLogic>();
                if(ball != null)
                {
                    ball.Pop();
                    numPopped++;
                }
            }
            if(numPopped > 0)
            {
                kill = !pierceDestructibles;
            }
        }

        var destructible = Physics2D.OverlapBox(boxPos, box.size, 0f, destructLayer);
        if (destructible != null)
        {
            var destructRef = destructible.GetComponentInParent<SingleDestructible>();
            if (destructRef != null)
            {
                destructRef.Hit();
                kill = !pierceDestructibles;
            }
            else
            {
                var chainRef = destructible.GetComponentInParent<ChainDestructible>();
                if (chainRef != null)
                {
                    var chainNode = destructible.GetComponent<ChainNode>();
                    if ((chainNode != null))
                    {
                        chainRef.Detonate(chainNode);
                        kill = !pierceDestructibles;
                    }
                }
            }
        }

        var indestructible = Physics2D.OverlapBox(boxPos, box.size, 0f, boundsLayer);
        if (indestructible != null)
        {
            kill = true;
        }

        if (kill)
        {
            Kill();
        }
        else
        {
            RefreshSize(curLineHeight + growSpeed * Time.deltaTime);
        }
    }

    public void Kill()
    {
        Destroyed?.Invoke(this);
        Destroy(gameObject);
    }
}
