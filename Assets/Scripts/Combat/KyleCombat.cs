using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KyleCombat : MonoBehaviour
{
    private float attackRange = 100f;
    private int attackDamage = 100;
    [SerializeField] private LayerMask enemyLayer;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Object.HasInputAuthority)
        {
            Punch();
        }

        if (Input.GetMouseButtonDown(1) && Object.HasInputAuthority)
        {
            Special();
        }
    }

    private void Punch()
    {
        if (Object.HasInputAuthority)
        {
            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

            if (hitEnemies.Length > 0)
            {
                foreach (Collider enemy in hitEnemies)
                {
                    if (enemy.TryGetComponent<NetworkObject>(out var networkObject))
                    {
                        InflictDamageRpc(networkObject.Id, attackDamage);
                    }
                }
            }
            else
            {
                Debug.Log($"{hitEnemies} is null");
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void InflictDamageRpc(NetworkId enemyId, int damage)
    {
        var enemy = Runner.TryFindObject(enemyId, out var networkObject) ? networkObject : null;

        if (enemy != null)
        {
            var healthComponent = enemy.GetComponent<Enemy>();
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(damage);
                Debug.Log($"Inflicted {damage} damage to {enemyId}");
            }
            else
            {
                Debug.LogWarning("Health component not found on enemy.");
            }
        }
        else
        {
            Debug.LogWarning("Enemy NetworkObject not found.");
        }
    }

    private void Special()
    {
        Debug.Log("Special");
    }
}
