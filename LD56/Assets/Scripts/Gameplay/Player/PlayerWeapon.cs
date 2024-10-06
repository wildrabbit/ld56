using System;
using UnityEngine;

public class PlayerWeapon: MonoBehaviour
{
    public float cooldown;
    public GameObject projectile;
    private PlayerLogic player;
    
    bool active = false;
    float elapsed = -1f;

    public event Action<PlayerWeapon> ShotsFired;
    public event Action<PlayerWeapon> Ready;
    public event Action<PlayerWeapon> Activated;
    public event Action<PlayerWeapon> Deactivated;

    void Init(PlayerLogic player, bool active = true)
    {
        this.player = player;
        elapsed = -1f;
        if (active)
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
        Activated?.Invoke(this);
    }

    public void Deactivate(bool purgeChildren = false)
    {
        active = false;
        Deactivated?.Invoke(this);
    }

    public bool TryShoot()
    {
        if(active && elapsed < 0)
        {
            // Spawn projectile
            elapsed = 0f;
            ShotsFired?.Invoke(this);
            return true;
        }
        return false;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if(elapsed >= 0f)
        {
            elapsed += dt;
            if(elapsed >= cooldown)
            {
                elapsed = -1f;
                Ready?.Invoke(this);
            }
        }
    }
}
