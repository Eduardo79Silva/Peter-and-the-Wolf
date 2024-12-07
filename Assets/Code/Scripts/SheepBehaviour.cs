using System;
using System.Collections.Generic;
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

    // Variables for gradual detection
    public float detectionTime = 2f;
    public float detectionDecayRate = 0.5f;
    private float currentDetectionLevel = 0f;

    // Variables for fence avoidance
    public string fenceTag = "Fence";
    public float fenceDetectionDistance = 5f;
    public float fenceAvoidanceStrength = 2f;

    private Vector3 gizmoTargetRotation;

    void Start()
    {
        BaseStart();
        speed = UnityEngine.Random.Range(FlockManager.FM.minSpeed, FlockManager.FM.maxSpeed);
    }

    void Update()
    {
        BaseUpdate();
        UpdateDetectionLevel();

        Vector3 fenceAvoidanceDirection = GetFenceAvoidanceDirection();

        if (isFleeing)
        {
            RunAwayFrom(wolf.transform, fenceAvoidanceDirection);
        }
        else
        {
            ApplyFlockingBehavior(fenceAvoidanceDirection);
        }
    }

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

    void OnWolfFullyDetected()
    {
        Debug.Log("Sheep fully detected a wolf! Running away!");
        isFleeing = true;
        WarnOtherSheep();
        fleeDirection = (transform.position - wolf.transform.position).normalized;
    }

    void ApplyFlockingBehavior(Vector3 fenceAvoidanceDirection)
    {
        if (UnityEngine.Random.Range(0, 100) < 1)
        {
            speed = UnityEngine.Random.Range(FlockManager.FM.minSpeed, FlockManager.FM.maxSpeed);
        }
        if (UnityEngine.Random.Range(0, 100) < 1)
        {
            ApplyRules();
        }

        Vector3 movement = transform.forward + fenceAvoidanceDirection * fenceAvoidanceStrength;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), rotationSpeed * Time.deltaTime);
        transform.Translate(0, 0, speed * Time.deltaTime);
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
                float nDistance = Vector3.Distance(sheep.transform.position, this.transform.position);
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

    protected override void OnTargetDetected()
    {
        base.OnTargetDetected();
        Debug.Log($"Sheep spotted a wolf! Detection level: {currentDetectionLevel:F2}/{detectionTime}");
    }

    private void RunAwayFrom(Transform wolf, Vector3 fenceAvoidanceDirection)
    {
        float distanceToWolf = Vector3.Distance(transform.position, wolf.position);
        if (distanceToWolf > safeDistance)
        {
            Debug.Log("Sheep is now at a safe distance. Stopping flee.");
            isFleeing = false;
            currentDetectionLevel = 0f; // Reset detection when safe
            return;
        }

        // Update flee direction, now including fence avoidance
        Vector3 directionAway = (transform.position - wolf.position).normalized;
        fleeDirection = Vector3.Lerp(fleeDirection, directionAway, Time.deltaTime * rotationSpeed);
        fleeDirection += fenceAvoidanceDirection * fenceAvoidanceStrength;
        fleeDirection.Normalize();

        // Random direction change
        if (UnityEngine.Random.value < directionChangeChance)
        {
            float randomAngle = UnityEngine.Random.Range(-maxDirectionChangeAngle, maxDirectionChangeAngle);
            fleeDirection = Quaternion.Euler(0, randomAngle, 0) * fleeDirection;
        }

        // Smooth rotation
        Quaternion targetRotation = Quaternion.LookRotation(fleeDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // Move the sheep away from the wolf at increased speed
        float fleeSpeed = speed * fleeSpeedMultiplier;
        transform.Translate(fleeSpeed * Time.deltaTime * Vector3.forward);
    }

    private void WarnOtherSheep()
    {
        foreach (GameObject sheep in FlockManager.FM.allSheep)
        {
            SheepBehaviour sheepBehaviour = sheep.GetComponent<SheepBehaviour>();
            sheepBehaviour.isFleeing = true;
        }
    }

    Vector3 GetFenceAvoidanceDirection()
    {
        Vector3 avoidanceDirection = Vector3.zero;
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, fenceDetectionDistance);

        foreach (Collider obj in nearbyObjects)
        {
            if (obj.CompareTag(fenceTag))
            {
                Vector3 awayFromFence = transform.position - obj.ClosestPoint(transform.position);
                awayFromFence.y = 0;
                avoidanceDirection += awayFromFence.normalized / awayFromFence.magnitude;
            }
        }

        return avoidanceDirection.normalized;
    }

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