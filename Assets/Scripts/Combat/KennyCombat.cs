using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KennyCombat : NetworkBehaviour
{
    private float attackRange = 2f;
    private int attackDamage = 20;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Slider healthbar;
    [SerializeField] private Slider powerbar;
    private int health = 100;
    private int power = 0;
    private bool isAttacking = false;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Object.HasInputAuthority)
        {
            Punch();
        }

        if (Input.GetMouseButtonDown(1) && Object.HasInputAuthority && power >= 100)
        {
            Special();
        }
    }

    private void Punch()
    {
        if (Object.HasInputAuthority)
        {
            isAttacking = true;
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
                isAttacking = false;
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

                if (power + 10 <= 100)
                {
                    power += 10;
                }
                else
                {
                    power = 100;
                }
                powerbar.value = power;
                isAttacking = false;
                Debug.Log($"Inflicted {damage} damage to {enemyId}");
            }
            else
            {
                Debug.LogWarning("Health component not found on enemy.");
            }

            Vector3 forceDirection = enemy.transform.position - transform.position;

            var rb = enemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                forceDirection.y = 1.0f;
                rb.AddForce(forceDirection.normalized * 3f, ForceMode.Impulse);
            }
        }
        else
        {
            Debug.LogWarning("Enemy NetworkObject not found.");
        }
    }

    private void Special()
    {
        power = 0;
        powerbar.value = power;

        if(health + 40 <= 100)
        {
            health += 20;
        }
        else
        {
            health = 100;
        }
        healthbar.value = health;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == enemyLayer && !isAttacking)
        {
            TakeDamage(15);
        }
    }

    private void TakeDamage(int damage)
    {
        health -= damage;
        healthbar.value = health;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"Kenny died");
        Runner.Despawn(Object);
        SceneManager.LoadScene("LooseScreen");
    }
}
