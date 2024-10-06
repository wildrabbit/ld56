using System;
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

    PlayerWeapon currentWeapon;
    PlayerMovement movement;
    PlayerInput input;

    public event Action<PlayerLogic, int> TookHit;
    public event Action<PlayerLogic> Died;

    public bool active = false;
    public bool invulnerable = false;
    int hp = 3;
    public Vector2 startPos;

    
    private void Awake()
    {
        startPos = transform.position;
        active = startActive;
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInput>();
    }

    public void StartGame()
    {
        active = true;
        movement.Init(moveSpeed, activate: true);
        input.Activate();
    }

    // Update is called once per frame
    void Update()
    {
        if(!active)
        {
            return;
        }

        // Update grace period, death, etc

        if(input.shootReleased && currentWeapon != null)
        {
            bool success = currentWeapon.TryShoot();
        }

        float dt = Time.deltaTime;
        movement.UpdateMove(dt, input.xAxis);
    }
}
