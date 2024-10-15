using System.Collections;
using UnityEngine;

public class PatrolBehaviour : EntityFOV
{
    public float patrolSpeed = 5f;
    public float chaseSpeed = 7f;
    public float waitTime = .3f;
    public float turnSpeed = 90f;

    public float detectionTime = 2f;
    public float detectionDecayRate = 0.5f;

    private bool isChasingTarget = false;
    private Vector3[] waypoints;
    private int currentWaypointIndex;
    private Coroutine activeCoroutine;

    private float currentDetectionLevel = 0f;

    public Transform pathHolder;

    void Start()
    {
        InitializeWaypoints();
        StartPatrolling();
    }

    void Update()
    {
        UpdateDetectionLevel();

        if (currentDetectionLevel >= detectionTime)
        {
            if (!isChasingTarget)
            {
                StartChasing();
            }
            ChaseTarget();
        }
        else if (isChasingTarget)
        {
            StopChasing();
            StartPatrolling();
        }
    }

    void UpdateDetectionLevel()
    {
        if (CanSeePlayer())
        {
            currentDetectionLevel += Time.deltaTime;
            currentDetectionLevel = Mathf.Min(currentDetectionLevel, detectionTime);
        }
        else
        {
            currentDetectionLevel -= detectionDecayRate * Time.deltaTime;
            currentDetectionLevel = Mathf.Max(currentDetectionLevel, 0f);
        }
    }

    void InitializeWaypoints()
    {
        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }
    }

    void StartPatrolling()
    {
        isChasingTarget = false;
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }
        activeCoroutine = StartCoroutine(Patrol());
    }

    void StartChasing()
    {
        isChasingTarget = true;
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }
        Debug.Log("Patrol started chasing the Wolf");
    }

    void StopChasing()
    {
        isChasingTarget = false;
        Debug.Log("Patrol stopped chasing the Wolf");
    }

    IEnumerator Patrol()
    {
        currentWaypointIndex = GetClosestWaypointIndex();

        while (true)
        {
            Vector3 targetWaypoint = waypoints[currentWaypointIndex];

            while (transform.position != targetWaypoint)
            {
                if (currentDetectionLevel >= detectionTime)
                {
                    yield break;
                }

                TurnToFace(targetWaypoint);
                transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, patrolSpeed * Time.deltaTime);
                yield return null;
            }

            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            yield return StartCoroutine(TurnToFace(waypoints[currentWaypointIndex]));
            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            if (currentDetectionLevel >= detectionTime)
            {
                yield break;
            }

            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    protected override void OnTargetDetected()
    {
        base.OnTargetDetected();
        Debug.Log($"Wolf in sight. Detection level: {currentDetectionLevel:F2}/{detectionTime}");
    }

    private void ChaseTarget()
    {
        if (wolf == null)
        {
            Debug.Log("Wolf is null");
            return;
        }

        Vector3 directionToTarget = (wolf.position - transform.position).normalized;
        directionToTarget.y = 0; // Ensure the guard doesn't move vertically
        Vector3 newPosition = transform.position + chaseSpeed * Time.deltaTime * directionToTarget;
        newPosition.y = transform.position.y; // Maintain the current y position
        transform.position = newPosition;

        // Rotate to face the wolf
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        float distanceToTarget = Vector3.Distance(transform.position, wolf.position);
        if (distanceToTarget < 1.5f)
        {
            Debug.Log("Patrol reached the Wolf");
            currentDetectionLevel = 0f; // Reset detection when guard reaches the wolf
            // Add code here for what happens when the patrol reaches the target (e.g., attack, alert, etc.)
        }
    }

    protected override bool IsObstructed(Collider target)
    {
        if (base.IsObstructed(target))
        {
            Debug.Log($"Target obstructed or lost. Detection level: {currentDetectionLevel:F2}/{detectionTime}");
            return true;
        }
        return false;
    }

    private int GetClosestWaypointIndex()
    {
        int closestIndex = 0;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < waypoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, waypoints[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    void OnDrawGizmos()
    {
        if (pathHolder == null) return;

        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);
        BaseOnDrawGizmos();

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * detectionDistance);

        // Draw detection meter
        Gizmos.color = Color.yellow;
        float detectionRatio = currentDetectionLevel / detectionTime;
        Vector3 meterPosition = transform.position + Vector3.up * 2f;
        Gizmos.DrawLine(meterPosition, meterPosition + Vector3.right * detectionRatio);
    }
}