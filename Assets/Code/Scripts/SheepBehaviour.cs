using System;
using UnityEngine;

public class SheepBehaviour : EntityFOV
{
    float speed;
    public float neighborDistance = 3f;
    public float fleeSpeedMultiplier = 2.5f;
    public float safeDistance = 50f;
    public float rotationSpeed = 3f;
    public float directionChangeChance = 0.02f;
    public float maxDirectionChangeAngle = 30f;
    private bool isFleeing = false;
    public bool isCaught = false;
    private Vector3 fleeDirection;

    // New variables for gradual detection
    public float detectionTime = 2f;
    public float detectionDecayRate = 0.5f;
    private float currentDetectionLevel = 0f;

    void Start()
    {
        BaseStart();
        speed = UnityEngine.Random.Range(FlockManager.FM.minSpeed, FlockManager.FM.maxSpeed);
    }

    void Update()
    {
        BaseUpdate();
        UpdateDetectionLevel();

        if (isFleeing)
        {
            RunAwayFrom(wolf.transform);
        }
        else
        {
            ApplyFlockingBehavior();
            transform.Translate(0, 0, speed * Time.deltaTime);
        }
    }

    // New method for updating detection level
    void UpdateDetectionLevel()
    {
        if (CanSeePlayer())
        {
            currentDetectionLevel += Time.deltaTime;
            currentDetectionLevel = Mathf.Min(currentDetectionLevel, detectionTime);

            if (currentDetectionLevel >= detectionTime && !isFleeing)
            {
                OnWolfFullyDetected();
            }
        }
        else
        {
            currentDetectionLevel -= detectionDecayRate * Time.deltaTime;
            currentDetectionLevel = Mathf.Max(currentDetectionLevel, 0f);
        }
    }

    // New method for when the wolf is fully detected
    void OnWolfFullyDetected()
    {
        Debug.Log("Sheep fully detected a wolf! Running away!");
        isFleeing = true;
        WarnOtherSheep();
        fleeDirection = (transform.position - wolf.transform.position).normalized;
    }

    void ApplyFlockingBehavior()
    {
        if (UnityEngine.Random.Range(0, 100) < 1)
        {
            speed = UnityEngine.Random.Range(FlockManager.FM.minSpeed, FlockManager.FM.maxSpeed);
        }
        if (UnityEngine.Random.Range(0, 100) < 1)
        {
            ApplyRules();
        }
    }

    void ApplyRules()
    {
        GameObject[] herd = FlockManager.FM.allSheep;

        Vector3 vCenter = Vector3.zero;
        Vector3 vAvoid = Vector3.zero;
        float gSpeed = 0.01f;
        int groupSize = 0;

        foreach (GameObject sheep in herd)
        {
            if (sheep != this.gameObject)
            {
                float nDistance = Vector3.Distance(
                    sheep.transform.position,
                    this.transform.position
                );
                if (nDistance <= FlockManager.FM.neighborDistance)
                {
                    vCenter += sheep.transform.position;
                    groupSize++;

                    if (nDistance < 1.0f)
                    {
                        vAvoid += this.transform.position - sheep.transform.position;
                    }

                    SheepBehaviour anotherSheep = sheep.GetComponent<SheepBehaviour>();
                    gSpeed += anotherSheep.speed;
                }
            }
        }

        if (groupSize > 0)
        {
            vCenter = vCenter / groupSize + (FlockManager.FM.goalPos - this.transform.position);
            speed = gSpeed / groupSize;

            Vector3 direction = vCenter + vAvoid - transform.position;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    FlockManager.FM.rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    // Modified to log the current detection level
    protected override void OnTargetDetected()
    {
        base.OnTargetDetected();
        Debug.Log($"Sheep spotted a wolf! Detection level: {currentDetectionLevel:F2}/{detectionTime}");
    }

    private void RunAwayFrom(Transform wolf)
    {
        float distanceToWolf = Vector3.Distance(transform.position, wolf.position);
        if (distanceToWolf > safeDistance)
        {
            Debug.Log("Sheep is now at a safe distance. Stopping flee.");
            isFleeing = false;
            currentDetectionLevel = 0f; // Reset detection when safe
            return;
        }

        // Update flee direction
        Vector3 directionAway = (transform.position - wolf.position).normalized;
        fleeDirection = Vector3.Lerp(fleeDirection, directionAway, Time.deltaTime * rotationSpeed);

        // Random direction change
        if (UnityEngine.Random.value < directionChangeChance)
        {
            float randomAngle = UnityEngine.Random.Range(
                -maxDirectionChangeAngle,
                maxDirectionChangeAngle
            );
            fleeDirection = Quaternion.Euler(0, randomAngle, 0) * fleeDirection;
        }

        // Smooth rotation
        Quaternion targetRotation = Quaternion.LookRotation(fleeDirection);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );

        // Move the sheep away from the wolf at increased speed
        float fleeSpeed = speed * fleeSpeedMultiplier;
        transform.Translate(Vector3.forward * fleeSpeed * Time.deltaTime);
    }

    private void WarnOtherSheep()
    {
        foreach (GameObject sheep in FlockManager.FM.allSheep)
        {
            SheepBehaviour sheepBehaviour = sheep.GetComponent<SheepBehaviour>();
            sheepBehaviour.isFleeing = true;
        }
    }

    // Optional: Add this method to visualize the detection meter
    void OnDrawGizmos()
    {
        BaseOnDrawGizmos();
        // Draw detection meter
        Gizmos.color = Color.yellow;
        float detectionRatio = currentDetectionLevel / detectionTime;
        Vector3 meterPosition = transform.position + Vector3.up * 2f;
        Gizmos.DrawLine(meterPosition, meterPosition + Vector3.right * detectionRatio);
    }
}