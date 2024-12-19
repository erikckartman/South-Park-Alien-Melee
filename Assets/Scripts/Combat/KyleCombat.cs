using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private void Start()
    {
        powerbar.value = power;
    }
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

                if (power + 10 <= 100)
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

    private void Special()
    {
        Debug.Log("Special");
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
                Vector3 launchDirection = spawnPoint.forward + Vector3.up * upwardForce;
                rb.AddForce(launchDirection.normalized * launchForce, ForceMode.Impulse);
                rb.AddTorque(new Vector3(-1f, 1f, 0f) * spinForce, ForceMode.Impulse);
            }
        });
    }
}
