using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class IkeScript : NetworkBehaviour
{
    private float lifeTime = 5f;

    public override void Spawned()
    {
        Invoke(nameof(DestroyIkeRpc), lifeTime);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void DestroyIkeRpc()
    {
        if (Runner.IsServer)
        {
            Runner.Despawn(Object);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(100);
        }
    }
}
