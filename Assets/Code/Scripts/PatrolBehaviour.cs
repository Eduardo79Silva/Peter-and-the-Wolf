using System.Collections;
using UnityEngine;

public class PatrolBehaviour : EntityFOV
{
    public float speed = 5;
    public float waitTime = .3f;
    public float turnSpeed = 90;

    private bool isChasingTarget = false;

    public Transform pathHolder;

    void Start()
    {
        Vector3[] waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        StartCoroutine(FollowPath(waypoints));
    }

    void Update()
    {
        if (CanSeePlayer())
        {
            ChaseTarget();
        }
    }

    IEnumerator FollowPath(Vector3[] waypoints)
    {
        transform.position = waypoints[0];

        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);

        while (true)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetWaypoint,
                speed * Time.deltaTime
            );
            if (transform.position == targetWaypoint)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetWaypoint));
            }
            yield return null;
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        while (Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(
                transform.eulerAngles.y,
                targetAngle,
                turnSpeed * Time.deltaTime
            );
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    // Override target detection logic for the patrol
    protected override void OnTargetDetected()
    {
        base.OnTargetDetected();
        Debug.Log("Patrol detected the Wolf");

        // Set chase mode
        isChasingTarget = true;
    }

    // Chase the detected target
    private void ChaseTarget()
    {
        if (wolf == null)
            return;

        // Move towards the target
        Vector3 directionToTarget = (wolf.position - transform.position).normalized;
        transform.position += directionToTarget * speed * Time.deltaTime;

        // Optional: You can add more logic for what happens when the patrol reaches the target
        float distanceToTarget = Vector3.Distance(transform.position, wolf.position);
        if (distanceToTarget < 1.5f) // Example: within a certain range
        {
            Debug.Log("Patrol reached the Wolf");
            // Add code here for what happens when the patrol reaches the target (e.g., attack, alert, etc.)
        }
    }

    // Reset chase mode if the target is lost (optional)
    protected override bool IsObstructed(Collider target)
    {
        if (base.IsObstructed(target))
        {
            Debug.Log("Target obstructed or lost.");
            isChasingTarget = false;
            return true;
        }
        return false;
    }

    void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * detectionDistance);
    }
}
