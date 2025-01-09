using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StanCombat : NetworkBehaviour
{
    private float attackRange = 2f;
    private int attackDamage = 20;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Slider healthbar;
    [SerializeField] private Slider powerbar;
    private int health = 100;
    private int power = 0;

    private float forwardForce = 15f;
    private float upwardForce = 5f;
    private float dashDuration = 0.2f;
    private bool isAttacking = false;

    [SerializeField] private ParticleSystem vomit;
    private int vomitDamage = 75;
    private List<Collider> damagedEnemies = new List<Collider>();

    [SerializeField] private AudioSource punchSound;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Object.HasInputAuthority)
        {
            StartCoroutine(Dash());
            Punch();
        }

        if (Input.GetMouseButtonDown(1) && Object.HasInputAuthority && power >= 100)
        {
            Special();
        }

        GetDamage(15);
    }

    private void Punch()
    {
        if (Object.HasInputAuthority)
        {
            isAttacking = true;
            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

            if(hitEnemies.Length > 0)
            {
                foreach (Collider enemy in hitEnemies)
                {
                    punchSound.Play();
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

    private void GetDamage(int damage)
    {
        if (Object.HasInputAuthority && !isAttacking)
        {
            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 1f, enemyLayer);

            if (hitEnemies.Length > 0)
            {
                foreach (Collider enemy in hitEnemies)
                {
                    if (!damagedEnemies.Contains(enemy))
                    {
                        damagedEnemies.Add(enemy);

                        Vector3 forceDirection = transform.position - enemy.transform.position;
                        var rb = GetComponent<Rigidbody>();                    
                        forceDirection.y = 0f;
                        rb.AddForce(forceDirection.normalized * 5f, ForceMode.Impulse);
                        
                        health -= damage;
                        healthbar.value = health;

                        if (health <= 0)
                        {
                            Die();
                        }
                    }
                }
                StartCoroutine(ClearDamagedEnemies());
            }
        }
    }

    private IEnumerator ClearDamagedEnemies()
    {
        yield return new WaitForSeconds(1.0f);
        damagedEnemies.Clear(); 
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

                if(power + 10 <= 100)
                {
                    power += 10;
                }
                else
                {
                    power = 100;
                }                
                powerbar.value = power;

                
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

    private IEnumerator Dash()
    {
        isAttacking = true;
        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            transform.Translate(Vector3.forward * forwardForce * Time.deltaTime);
            transform.Translate(Vector3.up * upwardForce * Time.deltaTime);
            yield return null;
        }
        if(Time.time >= startTime + dashDuration)
        {
            isAttacking = false;
        }
    }

    private void Special()
    {
        Debug.Log("Special");
        power = 0;
        powerbar.value = power;

        VomitEffectRpc();
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 30f, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            foreach (Collider enemy in hitEnemies)
            {
                if (enemy.TryGetComponent<NetworkObject>(out var networkObject))
                {
                    InflictDamageRpc(networkObject.Id, vomitDamage);
                }
            }
        }
    }

    private void Die()
    {
        Debug.Log($"Stan died");
        Runner.Despawn(Object);
        SceneManager.LoadScene("LooseScreen");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void VomitEffectRpc()
    {
        vomit.Play();
    }
}
