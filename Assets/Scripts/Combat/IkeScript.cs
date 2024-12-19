using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class IkeScript : NetworkBehaviour
{
    private TickTimer lifeTime;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            lifeTime = TickTimer.CreateFromSeconds(Runner, 5f);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && lifeTime.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

        private void OnCollisionEnter(Collision collision)
    {
        if (!Object.HasStateAuthority) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(100);
            }
        }
    }
}
