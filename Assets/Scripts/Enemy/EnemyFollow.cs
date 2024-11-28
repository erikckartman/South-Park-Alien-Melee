using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private Transform playerTransform;
    private float detectionRange = 10f;
    private float rotationSpeed = 5f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
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
}
