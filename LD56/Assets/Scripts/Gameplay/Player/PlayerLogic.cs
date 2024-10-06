using System;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.UIElements;

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
    public Vector2 startPos;
    LayerMask ballsMask;

    GameResult gameResult;
    
    private void Awake()
    {
        startPos = transform.position;
        active = startActive;
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInput>();
        ballsMask = LayerMask.GetMask("Balls");
    }

    public void StartGame()
    {
        active = true;
        movement.Init(moveSpeed, activate: true);
        input.Activate();
        invulnerableElapsed = -1f;
        hp = numHP;
        if (defaultWeapon != null)
        {
            currentWeapon = defaultWeapon;
            currentWeapon.Init(this, true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!active)
        {
            return;
        }
        float dt = Time.deltaTime;
        UpdateInvulnerable(dt);

        if(invulnerableElapsed < 0f)
        {
            Vector2 boxPos = (Vector2)lifeCollider.transform.position + lifeCollider.offset;

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

        // Update grace period, death, etc

        if(input.shootReleased && currentWeapon != null)
        {
            bool success = currentWeapon.TryShoot();
            if (success)
            {
                Debug.Log($"<color=cyan>PLAYER</color>Pew, pew!");
            }
        }

        movement.UpdateMove(dt, input.xAxis);
    }

    private bool TakeHit()
    {
        hp = Mathf.Max(hp - 1, 0);
        if (hp == 0)
        {
            Died?.Invoke(this);
            input.Deactivate();
            movement.Deactivate();
            // visuals;
            return true;
        }
        else
        {
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
