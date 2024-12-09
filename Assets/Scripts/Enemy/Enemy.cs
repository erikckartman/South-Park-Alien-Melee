using Fusion;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private bool canGo = false;
    private Transform playerTransform;
    private float detectionRange = 10f;
    private float rotationSpeed = 5f;
    public Player[] players;

    [Networked] private Vector3 TargedPosition { get; set; }

    public void Start()
    {
        if (Runner.IsServer)
        {
            agent.enabled = false;
            SearchForPlayers();
        }
    }

    public void Update()
    {
        if (Runner.IsServer)
        {
            UpdateEnemy();
        }
        else
        {
            FollowTarget();
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
            agent.SetDestination(playerTransform.position);
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

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
