using System.Collections;
using UnityEngine;

public class PatrolBehaviour : EntityFOV
{
    public float patrolSpeed = 5f;
    public float chaseSpeed = 7f;
    public float waitTime = .3f;
    public float turnSpeed = 90f;
    public float scanAngle = 30f;
    public float scanSpeed = 45f;
    public int scanCycles = 1;
    [Range(0f, 1f)]
    public float scanProbability = 0.6f;
    public float minWaypointsBetweenScans = 2f;

    public float detectionTime = 2f;
    public float detectionDecayRate = 0.5f;

    private bool isChasingTarget = false;
    private Vector3[] waypoints;
    private int currentWaypointIndex;
    private Coroutine activeCoroutine;
    private Vector3 pathCenter;
    private int waypointsSinceLastScan = 0;

    private float currentDetectionLevel = 0f;

    public Transform pathHolder;

    void Start()
    {
        BaseStart();
        InitializeWaypoints();
        CalculatePathCenter();
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
            float distanceToWolf = Vector3.Distance(transform.position, wolf.position);
            float inverseProximity = 1f - Mathf.Clamp01(distanceToWolf / detectionDistance);
            detectionTime = Mathf.Lerp(0.5f, 2f, inverseProximity);
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

    void CalculatePathCenter()
    {
        if(waypoints.Length == 0)
        {
            return;
        }

        if(waypoints.Length == 1)
        {
            pathCenter = waypoints[0] ;
            return;
        }

        Vector3 sum = Vector3.zero;
        foreach (Vector3 waypoint in waypoints)
        {
            sum += waypoint;
        }
        pathCenter = sum / waypoints.Length;
        pathCenter.y = transform.position.y;
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
        waypointsSinceLastScan = 0;

        while (true)
        {
            Vector3 targetWaypoint = waypoints[currentWaypointIndex];

            while (transform.position != targetWaypoint)
            {
                if (currentDetectionLevel >= detectionTime)
                {
                    yield break;
                }

                StartCoroutine(TurnToFace(targetWaypoint));
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetWaypoint,
                    patrolSpeed * Time.deltaTime
                );
                yield return null;
            }

            // Decide whether to scan based on probability and minimum waypoint requirement
            bool shouldScan = Random.value < scanProbability && waypointsSinceLastScan >= minWaypointsBetweenScans;
            
            if (shouldScan)
            {
                yield return StartCoroutine(ScanInterior());
                waypointsSinceLastScan = 0;
            }
            else
            {
                waypointsSinceLastScan++;
            }
            
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            yield return StartCoroutine(TurnToFace(waypoints[currentWaypointIndex]));
            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator ScanInterior()
    {
        // Face the center of the path
        Vector3 directionToCenter = pathCenter - transform.position;
        yield return StartCoroutine(TurnToFace(transform.position + directionToCenter));

        float baseAngle = transform.eulerAngles.y;
        
        // Perform scanning cycles
        for (int cycle = 0; cycle < scanCycles; cycle++)
        {
            // Look right
            float targetAngle = baseAngle + scanAngle;
            while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
            {
                if (currentDetectionLevel >= detectionTime)
                    yield break;

                float angle = Mathf.MoveTowardsAngle(
                    transform.eulerAngles.y,
                    targetAngle,
                    scanSpeed * Time.deltaTime
                );
                transform.eulerAngles = Vector3.up * angle;
                yield return null;
            }

            // Look left
            targetAngle = baseAngle - scanAngle;
            while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
            {
                if (currentDetectionLevel >= detectionTime)
                    yield break;

                float angle = Mathf.MoveTowardsAngle(
                    transform.eulerAngles.y,
                    targetAngle,
                    scanSpeed * Time.deltaTime
                );
                transform.eulerAngles = Vector3.up * angle;
                yield return null;
            }
        }

        // Return to center
        yield return StartCoroutine(TurnToFace(transform.position + directionToCenter));
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

            float angle = Mathf.MoveTowardsAngle(
                transform.eulerAngles.y,
                targetAngle,
                turnSpeed * Time.deltaTime
            );
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    protected override void OnTargetDetected()
    {
        base.OnTargetDetected();
    }

    private void ChaseTarget()
    {
        if (wolf == null)
        {
            return;
        }

        Vector3 directionToTarget = (wolf.position - transform.position).normalized;
        directionToTarget.y = 0; // Ensure the guard doesn't move vertically
        Vector3 newPosition = transform.position + chaseSpeed * Time.deltaTime * directionToTarget;
        newPosition.y = transform.position.y; // Maintain the current y position
        transform.position = newPosition;

        // Rotate to face the wolf
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );

        float distanceToTarget = Vector3.Distance(transform.position, wolf.position);
        if (distanceToTarget < 1.5f)
        {
            currentDetectionLevel = 0f; // Reset detection when guard reaches the wolf
        }
    }

    protected override bool IsObstructed(Collider target)
    {
        if (base.IsObstructed(target))
        {
            Debug.Log(
                $"Target obstructed or lost. Detection level: {currentDetectionLevel:F2}/{detectionTime}"
            );
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
        if (pathHolder == null)
            return;

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

        // Draw path center visualization
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pathCenter, 0.5f);
        }
    }
}