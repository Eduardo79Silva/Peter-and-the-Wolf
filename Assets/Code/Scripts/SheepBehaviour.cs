using System;
using UnityEngine;

public class SheepBehaviour : MonoBehaviour
{
    float speed;
    public float neighborDistance = 3f;
    public float fleeSpeedMultiplier = 2f;

    // Field of view settings
    public float detectionRadius = 10f;
    public float fieldOfViewAngle = 120f; // 120-degree FOV

    private Vector3 direction;
    private bool isFleeing = false;

    void Start()
    {
        speed = UnityEngine.Random.Range(FlockManager.FM.minSpeed, FlockManager.FM.maxSpeed);
        direction = UnityEngine.Random.insideUnitSphere.normalized; // Random starting direction
    }

    void Update()
    {
        if (UnityEngine.Random.Range(0, 100) < 1)
        {
            speed = UnityEngine.Random.Range(FlockManager.FM.minSpeed, FlockManager.FM.maxSpeed);
        }
        ApplyRules();
        this.transform.Translate(0, 0, speed * Time.deltaTime);
    }

    void ApplyRules()
    {
        GameObject[] herd;
        herd = FlockManager.FM.allSheep;

        Vector3 vCenter = Vector3.zero;
        Vector3 vAvoid = Vector3.zero;
        float gSpeed = 0.01f;
        float nDistance;
        int groupSize = 0;

        foreach (GameObject sheep in herd)
        {
            if (sheep != this.gameObject)
            {
                nDistance = Vector3.Distance(sheep.transform.position, this.transform.position);
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

            Debug.Log("Direction:" + direction);
            Debug.Log("FlockMates:" + groupSize);

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

    public void MoveSheep()
    {
        Vector3 averageHeading = Vector3.zero;
        Vector3 fleeDirection = Vector3.zero;
        int flockMates = 0;

        foreach (GameObject sheep in FlockManager.FM.allSheep)
        {
            float distance = Vector3.Distance(sheep.transform.position, transform.position);

            // Flocking behavior (moving with the herd)
            if (distance > 0 && distance < neighborDistance)
            {
                averageHeading += sheep.transform.forward;
                flockMates++;
            }
        }

        if (isFleeing)
        {
            direction = fleeDirection * fleeSpeedMultiplier;
        }
        else if (flockMates > 0)
        {
            direction = (averageHeading / flockMates).normalized;
        }

        // Move the sheep in the determined direction
        transform.SetPositionAndRotation(
            speed * Time.deltaTime * transform.forward,
            Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                Time.deltaTime * 2f
            )
        );
    }
}
