using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] BoxCollider2D bichiCollectCollider;
    
    [SerializeField] Animator animator;


    public bool Dead => hp == 0;
    public Transform groundAttachment;
    public int BichisRescued => bichisRescued;

    PlayerWeapon currentWeapon;
    PlayerMovement movement;
    PlayerInput input;

    public event Action<PlayerLogic, int> TookHit;
    public event Action<PlayerLogic> LostAllHealth;
    public event Action<PlayerLogic> Died;
    public event Action<PlayerLogic, int> CollectedBichis;

    public bool active = false;
    public float invulnerableElapsed = -1f;
    int hp = 3;
    int bichisRescued;
    public Vector2 startPos;
    LayerMask ballsMask;
    LayerMask bichisMask;
    List<Collider2D> bichiColliders = new List<Collider2D>();

    GameResult gameResult;
    private int idle;
    private int left;
    private int right;
    private int shoot;
    private int hit;
    private int dead;
    private int win;

    private void Awake()
    {
        idle = Animator.StringToHash("Idle");
        left = Animator.StringToHash("Left");
        right = Animator.StringToHash("Right");
        shoot = Animator.StringToHash("Shoot");
        hit = Animator.StringToHash("Hit");
        dead = Animator.StringToHash("Dead");
        win = Animator.StringToHash("Win");

        startPos = transform.position;
        active = startActive;
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInput>();
        ballsMask = LayerMask.GetMask("Balls");
        bichisMask = LayerMask.GetMask("Bichis");
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
        animator.Rebind();
        animator.Play(idle);
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
            var ballHit = Physics2D.OverlapBox(boxPos, lifeCollider.size, 0f, ballsMask);
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
                    ball.Pop(playerHit:true);
                }
            }
        }

        var colliders = Physics2D.OverlapBoxAll((Vector2)bichiCollectCollider.transform.position + bichiCollectCollider.offset, bichiCollectCollider.size, 0f, bichisMask);
        // Physics2D.OverlapCollider(bichiCollectCollider, bichisFilter, bichiColliders);
        if (colliders.Length > 0)
        {
            int oldRescued = bichisRescued;
            foreach(var bichi in colliders)
            {
                var bichiLogic = bichi.gameObject.GetComponentInParent<BichiLogic>();
                if(bichiLogic != null)
                {
                    bichisRescued++;
                    bichiLogic.Kill();
                    Debug.Log($"<color=cyan>[PLAYER]</color> Picked a bichi! Rescued: {bichisRescued} 🐣");
                }
            }
            if(oldRescued < bichisRescued)
            {
                CollectedBichis?.Invoke(this, bichisRescued);
            }
        }

        if(input.shootReleased && currentWeapon != null)
        {
            bool success = currentWeapon.TryShoot();            
            if(success)
            {
                animator.Play(shoot);
            }
        }

        var velocity = movement.velocity;
        movement.UpdateMove(dt, input.xAxis);
        var newVel = movement.velocity;
        bool zeroVel = Mathf.Approximately(newVel.magnitude, 0f);
        if (newVel != velocity)
        {
            if (zeroVel)
            {
                animator.Play(idle);
            }
            else if (Mathf.Sign(newVel.x) != Mathf.Sign(velocity.x) || Mathf.Approximately(velocity.magnitude, 0f))
            {
                if(newVel.x > 0)
                {
                    animator.Play(right);
                }
                else animator.Play(left);
            }
        }
        else
        {
            if (!zeroVel && (IsPlayingAnimation(idle) || IsPlayingAnimation(shoot)))
            {
                if (newVel.x > 0)
                {
                    animator.Play(right);
                }
                else animator.Play(left);
            }
        }

    }

    private bool IsPlayingAnimation(int hash)
    {
        bool animatorPlaying = animator.GetCurrentAnimatorStateInfo(0).length >
                   animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        int shortH = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        return animatorPlaying && shortH == hash;
        
    }

    private bool TakeHit(int dmg = 1)
    {
        hp = Mathf.Max(hp - dmg, 0);
        Debug.Log($"<color=cyan>[PLAYER]</color> Ouch!");
        if (hp == 0)
        {
            Debug.Log($"<color=cyan>[PLAYER]</color> DIED ☠️");
            animator.Play(dead);
            LostAllHealth?.Invoke(this);
            Deactivate();
            StartCoroutine(DelayEvent());
            // visuals;
            return true;
        }
        else
        {
            Debug.Log($"<color=cyan>[PLAYER]</color> DMG {hp}/{numHP} 🤕");
            animator.Play(hit, 1);
            TookHit?.Invoke(this, hp);
            if(gracePeriodOnHit > 0f)
            {
                invulnerableElapsed = 0f;
            }
            return false;
        }
        
    }

    private IEnumerator DelayEvent()
    {
        yield return new WaitForSeconds(1f);
        Died?.Invoke(this);
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

    public void PlayWin()
    {
        animator.Play(win);
    }

    public void Hit(int dmg)
    {
        if (invulnerableElapsed < 0f)
        {
            TakeHit(dmg);
        }            
    }
}
