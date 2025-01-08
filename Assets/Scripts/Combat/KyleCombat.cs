using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KyleCombat : NetworkBehaviour
{
    private float attackRange = 2f;
    private int attackDamage = 20;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Slider healthbar;
    [SerializeField] private Slider powerbar;
    private int health = 100;
    private int power = 100;

    [Header("Ike")]
    private float launchForce = 15f;
    private float spinForce = 10f;
    private float upwardForce = 2f;

    [SerializeField] private GameObject ikePrefab;
    [SerializeField] private Transform spawnPoint;
    [Networked] private NetworkObject currentIke { get; set; }
    [Networked] private Vector3 IkeVelocity { get; set; }
    [Networked] private Vector3 IkeAngularVelocity { get; set; }
    private bool isAttacking = false;

    private float forwardForce = 15f;
    private float upwardIkeForce = 5f;
    private float dashDuration = 0.2f;
    private List<Collider> damagedEnemies = new List<Collider>();

    private void Start()
    {
        powerbar.value = power;
    }
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

    private void FixedUpdate()
    {
        if (currentIke != null && !Object.HasInputAuthority)
        {
            currentIke.transform.position = Vector3.Lerp(
                currentIke.transform.position,
                currentIke.transform.position + IkeVelocity * Time.fixedDeltaTime,
                0.1f
            );

            currentIke.transform.rotation = Quaternion.Lerp(
                currentIke.transform.rotation,
                Quaternion.Euler(IkeAngularVelocity * Time.fixedDeltaTime),
                0.1f
            );
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
        LaunchIke();
    }

    public void LaunchIke()
    {
        if (currentIke != null)
        {
            Debug.LogWarning("Ike already launched!");
            return;
        }

        Runner.Spawn(ikePrefab, spawnPoint.position, spawnPoint.rotation, Object.InputAuthority, (runner, obj) =>
        {
            currentIke = obj.GetComponent<NetworkObject>();
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 launchDirection = spawnPoint.forward + Vector3.up * upwardIkeForce;
                rb.AddForce(launchDirection.normalized * launchForce, ForceMode.Impulse);
                rb.AddTorque(new Vector3(-1f, 1f, 0f) * spinForce, ForceMode.Impulse);
            }
        });
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
