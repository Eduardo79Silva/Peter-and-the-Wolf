using UnityEngine;

public class SheepBehaviour : MonoBehaviour
{
    public FlockManager flockManager;
    public float speed = 2f;
    public float neighborDistance = 3f;
    public float fleeSpeedMultiplier = 2f;

    // Field of view settings
    public float detectionRadius = 10f;
    public float fieldOfViewAngle = 120f; // 120-degree FOV

    private Vector3 direction;
    private bool isFleeing = false;

    void Start()
    {
        direction = Random.insideUnitSphere.normalized; // Random starting direction
    }

    public void MoveSheep()
    {
        Vector3 averageHeading = Vector3.zero;
        Vector3 fleeDirection = Vector3.zero;
        int flockMates = 0;

        foreach (GameObject sheep in flockManager.sheepArray)
        {
            float distance = Vector3.Distance(sheep.transform.position, transform.position);

            // Flocking behavior (moving with the herd)
            if (distance > 0 && distance < neighborDistance)
            {
                averageHeading += sheep.transform.forward;
                flockMates++;
            }

            // Detect Wolf with field of view (FOV)
            if (IsWolfInFieldOfView())
            {
                fleeDirection = (transform.position - flockManager.wolfTransform.position).normalized;
                isFleeing = true;
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
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 2f);
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    bool IsWolfInFieldOfView()
    {
        Vector3 directionToWolf = flockManager.wolfTransform.position - transform.position;
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
