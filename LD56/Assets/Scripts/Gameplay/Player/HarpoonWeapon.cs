﻿using System.Collections.Generic;
using UnityEngine;

public class HarpoonWeapon : PlayerWeapon
{
    [SerializeField] HarpoonProjectile prefab;
    [SerializeField] int instanceLimit = 1;
    [SerializeField] AudioSource sfxSrcRef;

    List<HarpoonProjectile> instances = new();

    protected override bool DoTryShoot()
    {
        if (instances.Count == instanceLimit)
        {
            return false;
        }

        var newInstance = Instantiate<HarpoonProjectile>(prefab, null);
        newInstance.shootSfx = sfxSrcRef;
        newInstance.transform.position = player.groundAttachment.position;
        newInstance.Destroyed += OnDestroyedProjectile;
        newInstance.Init(player);
        instances.Add(newInstance);
        return true;
    }

    private void OnDestroyedProjectile(HarpoonProjectile projectile)
    {
        projectile.Destroyed -= OnDestroyedProjectile;
        instances.Remove(projectile);
    }

    protected override void DoDeactivate()
    { 
        foreach(var i in instances)
        {
            i.Destroyed -= OnDestroyedProjectile;
            i.Kill();
        }
        instances.Clear();
    }
}
