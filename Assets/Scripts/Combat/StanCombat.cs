﻿using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanCombat : NetworkBehaviour
{
    private float attackRange = 2f;
    private int attackDamage = 20;
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

            if(hitEnemies.Length > 0)
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

            //Vector3 forceDirection = (enemy.transform.position - transform.position).normalized;

            //var rb = enemy.GetComponent<Rigidbody>();
            //if (rb != null)
            //{
            //    Vector3 force = forceDirection.normalized * 5f;
            //    rb.AddForce(force, ForceMode.Impulse);
            //}
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
