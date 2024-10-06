using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public enum Facing
{
    Left,
    Right,
    Idle,
    PointLeft,
    PointRight
}

public class PlayerLogic : MonoBehaviour
{
    [SerializeField] float moveSpeed = 2.5f;
    [SerializeField] int numHP = 3;
    [SerializeField] bool startActive = false;
    [SerializeField] PlayerWeapon defaultWeapon;
    [SerializeField] float gracePeriodOnHit = 2f;
    [SerializeField] BoxCollider2D lifeCollider;
    [SerializeField] Collider2D bichiCollectCollider;
    public bool Dead => hp == 0;
    public Transform groundAttachment;

    PlayerWeapon currentWeapon;
    PlayerMovement movement;
    PlayerInput input;

    public event Action<PlayerLogic, int> TookHit;
    public event Action<PlayerLogic> Died;

    public bool active = false;
    public float invulnerableElapsed = -1f;
    int hp = 3;
    int bichisRescued;
    public Vector2 startPos;
    LayerMask ballsMask;
    ContactFilter2D bichisFilter;
    List<Collider2D> bichiColliders = new List<Collider2D>();

    GameResult gameResult;
    
    private void Awake()
    {
        startPos = transform.position;
        active = startActive;
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInput>();
        bichisFilter = new ContactFilter2D()
        {
            useLayerMask = true,
            layerMask = LayerMask.GetMask("Bichis")
        };
    }

    public void StartGame()
    {
        active = true;
        movement.Init(moveSpeed, activate: true);
        input.Activate();
        invulnerableElapsed = -1f;
        hp = numHP;
        bichisRescued = 0;
        bichiColliders.Clear();
        if (defaultWeapon != null)
        {
            currentWeapon = defaultWeapon;
            currentWeapon.Init(this, true);
        }
    }

    public void Activate()
    {
        if (!Dead)
        {
            active = true;
            movement.Activate();
            input.Activate();
            if (currentWeapon != null)
            {
                currentWeapon.Activate();
            }
        }
    }

    public void Deactivate()
    {
        active = false;
        movement.Deactivate();
        input.Deactivate();
        if (currentWeapon != null)
        {
            currentWeapon.Deactivate();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(!active || Dead)
        {
            return;
        }
        float dt = Time.deltaTime;
        UpdateInvulnerable(dt);

        Vector2 boxPos = (Vector2)lifeCollider.transform.position + lifeCollider.offset;
        if (invulnerableElapsed < 0f)
        {
            var ballHit = Physics2D.OverlapBox(boxPos, lifeCollider.size, ballsMask);
            if (ballHit != null)
            {
                var ball = ballHit.GetComponentInParent<BallLogic>();
                if (ball != null)
                {
                    bool died = TakeHit();
                    if (died)
                    {
                        return;
                    }
                    ball.Pop();
                }
            }
        }

        Physics2D.OverlapCollider(bichiCollectCollider, bichisFilter, bichiColliders);
        if (bichiColliders.Count > 0)
        {
            foreach(var bichi in bichiColliders)
            {
                var bichiLogic = bichi.gameObject.GetComponentInParent<BichiLogic>();
                if(bichiLogic != null)
                {
                    bichisRescued++;
                    bichiLogic.Kill();
                    Debug.Log($"<color=cyan>[PLAYER]</color> Picked a bichi! Rescued: {bichisRescued} 🐣");
                }
            }
        }

        if(input.shootReleased && currentWeapon != null)
        {
            bool success = currentWeapon.TryShoot();            
        }

        movement.UpdateMove(dt, input.xAxis);
    }

    private bool TakeHit()
    {
        hp = Mathf.Max(hp - 1, 0);
        Debug.Log($"<color=cyan>[PLAYER]</color> Ouch!");
        if (hp == 0)
        {
            Debug.Log($"<color=cyan>[PLAYER]</color> DIED ☠️");
            Died?.Invoke(this);
            input.Deactivate();
            movement.Deactivate();
            // visuals;
            return true;
        }
        else
        {
            Debug.Log($"<color=cyan>[PLAYER]</color> DMG {hp}/{numHP} 🤕");
            TookHit?.Invoke(this, hp);
            if(gracePeriodOnHit > 0f)
            {
                invulnerableElapsed = 0f;
            }
            return false;
        }
        
    }

    private void UpdateInvulnerable(float dt)
    {
        if (invulnerableElapsed >= 0f)
        {
            invulnerableElapsed += dt;
            if (invulnerableElapsed >= gracePeriodOnHit)
            {
                invulnerableElapsed = -1f;
            }
        }
    }
}
