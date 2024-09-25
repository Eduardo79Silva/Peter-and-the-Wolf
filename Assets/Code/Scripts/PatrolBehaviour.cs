using UnityEngine;

public class PatrolBehaviour : MonoBehaviour
{
    public Transform[] waypoints;
    public float patrolSpeed = 3f;
    private int currentWaypointIndex = 0;

    // Detection settings
    public float detectionRadius = 8f;
    public float fieldOfViewAngle = 90f; // 90-degree FOV
    public Transform wolfTransform;

    private bool chasingWolf = false;

    void Update()
    {
        if (chasingWolf)
        {
            ChaseWolf();
        }
        else
        {
            Patrol();
            DetectWolf();
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        // Move towards the current waypoint
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, patrolSpeed * Time.deltaTime);

        // If close enough to the waypoint, move to the next one
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void DetectWolf()
    {
        if (IsWolfInFieldOfView())
        {
            chasingWolf = true;
        }
    }

    void ChaseWolf()
    {
        // Move towards the Wolf if detected
        Vector3 direction = (wolfTransform.position - transform.position).normalized;
        transform.position += direction * patrolSpeed * Time.deltaTime;

        // If Wolf escapes detection range, return to patrol
        if (Vector3.Distance(transform.position, wolfTransform.position) > detectionRadius * 2)
        {
            chasingWolf = false;
        }
    }

    bool IsWolfInFieldOfView()
    {
        Vector3 directionToWolf = wolfTransform.position - transform.position;
        float distanceToWolf = directionToWolf.magnitude;

        // Check if Wolf is within detection radius
        if (distanceToWolf <= detectionRadius)
        {
            // Check if Wolf is within the field of view (FOV)
            float angleToWolf = Vector3.Angle(transform.forward, directionToWolf.normalized);
            if (angleToWolf < fieldOfViewAngle / 2)
            {
                return true; // Wolf is within FOV
            }
        }

        return false; // Wolf is outside FOV or detection radius
    }
}
