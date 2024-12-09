using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform playerTransform;
    private float detectionRange = 10f;
    private float rotationSpeed = 5f;
    public Player[] players;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
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

        if (playerTransform != null)
        {
            agent.SetDestination(playerTransform.position);
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            RotateTowardsPlayer();
        }
    }
    private void RotateTowardsPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(collision.gameObject);
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

    public void SearchForPlayers()
    {
        players = FindObjectsOfType<Player>();
    }
}
