using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EricCombat : NetworkBehaviour
{
    private float attackRange = 2f;
    private int attackDamage = 20;

    [Header("General stuff")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Slider healthbar;
    [SerializeField] private Slider powerbar;
    private int health = 100;
    private int power = 0;

    [Header("Special attack parametrs")]
    [SerializeField] private LineRenderer lightningRenderer;
    private GameObject target;
    [SerializeField] private AudioSource swearingSound;
    private float lightningDuration = 0.5f;

    private float forwardForce = 15f;
    private float upwardForce = 5f;
    private float dashDuration = 0.2f;
    private bool isAttacking = false;
    private List<Collider> damagedEnemies = new List<Collider>();

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
        FindClosestTarget();
        if (target != null)
        {
            RPC_TriggerLightning();
        }
        else
        {
            Debug.Log($"Targer is {target}");
        }     
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_TriggerLightning()
    {
        StartCoroutine(PlayLightning());

        var enemyHealth = target.GetComponent<Enemy>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(100);
        }
    }

    private System.Collections.IEnumerator PlayLightning()
    {
        swearingSound.Play();

        Vector3 start = transform.position; 
        Vector3 end = target.transform.position;

        lightningRenderer.positionCount = 10; 
        for (int i = 0; i < lightningRenderer.positionCount; i++)
        {
            float t = i / (float)(lightningRenderer.positionCount - 1);
            Vector3 point = Vector3.Lerp(start, end, t);
            point += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
            lightningRenderer.SetPosition(i, point);
        }

        lightningRenderer.enabled = true;

        yield return new WaitForSeconds(lightningDuration);

        lightningRenderer.enabled = false;
    }

    private void FindClosestTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 60f, enemyLayer);
        float closestDistance = Mathf.Infinity;
        GameObject closestTarget = null;

        foreach (Collider col in hitColliders)
        {
            Vector3 directionToTarget = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = col.gameObject;
            }
        }

        target = closestTarget;
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

    private void Die()
    {
        Debug.Log($"Kyle died");
        Runner.Despawn(Object);
        SceneManager.LoadScene("LooseScreen");
    }
}
