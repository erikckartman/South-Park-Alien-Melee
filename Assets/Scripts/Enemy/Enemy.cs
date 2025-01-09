using Fusion;
using System.Globalization;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Enemy : NetworkBehaviour
{
    [Networked] private int health { get; set; }
    private int maxHealth = 100;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private bool canGo = false;
    private Transform playerTransform;
    private float detectionRange = 10f;
    private float rotationSpeed = 5f;
    public Player[] players;
    private Spawner spawner;
    [Networked] private Vector3 EnemyPosition { get; set; }
    [Networked] private Vector3 EnemyVelocity { get; set; }
    [Networked] private Vector3 TargedPosition { get; set; }
    private Vector3 targetPosition;
    [SerializeField] private GameObject mesh;
    private Vector3 meshScale;

    private void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
    }

    public void Start()
    {
        health = maxHealth;

        if (Runner.IsServer)
        {
            agent.enabled = false;
            SearchForPlayers();
        }

        InvokeRepeating("Animation", 0, 1);
    }

    public void FixedUpdate()
    {
        if (!Object.HasStateAuthority)
        {
            targetPosition = EnemyPosition;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5);
        }

        if (transform.position.y <= -5f)
        {
            Runner.Despawn(Object);
        }
    }

    public void TakeDamage(int damage)
    {
        if (Object.HasStateAuthority)
        {
            health -= damage;
            Debug.Log($"Enemy took {damage} damage. Remaining health: {health}");
        } 

        if (health <= 0)
        {
            Die();
        }
    }

    private void Animation(){
        if(health <= 0) return;

        meshScale = mesh.transform.localScale;
        meshScale.x *= -1;
        mesh.transform.localScale = meshScale;
    }

    private void Die()
    {
        Debug.Log("Enemy died!");
        spawner.currentEnemies--;
        spawner.UpdateUI();
        Runner.Despawn(Object);
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            UpdateEnemy();
        }
        else
        {
            transform.position = EnemyPosition;
        }
    }

    private void UpdateEnemy()
    {
        if (players != null && players.Length > 0)
        {
            Player nearestPlayer = GetNearestPlayer(transform.position);

            if (nearestPlayer != null)
            {
                playerTransform = nearestPlayer.transform;

            }
            else
            {
                playerTransform = null;
            }
        }

        if (playerTransform != null && canGo)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            EnemyVelocity = direction * 3f;

            EnemyPosition = transform.position + EnemyVelocity * Runner.DeltaTime;
            transform.position = EnemyPosition;

            RotateTowardsPlayer();
        }
    }

    private void FollowTarget()
    {
        if (playerTransform.position != Vector3.zero && canGo)
        {
            agent.SetDestination(playerTransform.position);
        }
    }

    private void SearchForPlayers()
    {
        players = FindObjectsOfType<Player>();
    }

    private void RotateTowardsPlayer()
    {
        if (playerTransform == null) return;

        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            agent.enabled = true;
            canGo = true;
        }
    }

    private Player GetNearestPlayer(Vector3 position)
    {
        Player nearestPlayer = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Player player in players)
        {
            float distance = Vector3.Distance(position, player.transform.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestPlayer = player;
            }
        }

        return nearestPlayer;
    }
}
