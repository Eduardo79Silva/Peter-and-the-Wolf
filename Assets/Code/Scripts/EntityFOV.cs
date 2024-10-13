using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityFOV : MonoBehaviour
{
    // Public variables for configuring the FOV
    public float detectionDistance = 10f; // Max distance the entity can detect
    public float fovAngle = 90f; // Cone angle (in degrees) for FOV

    // Reference to the target tag (e.g. "Wolf")
    public string targetTag = "Wolf";

    public LayerMask viewMask;

    // To track if a target is currently detected
    protected bool targetDetected = false;

    public Transform wolf;

    protected void BaseStart()
    {
        wolf = GameObject.FindGameObjectWithTag("Wolf").transform;
        Debug.Log(wolf.name);
    }

    protected void BaseUpdate()
    {
        targetDetected = CanSeePlayer();
    }

    // Method to detect targets within the field of view
    protected virtual bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, wolf.position) < detectionDistance)
        {
            Vector3 dirToPlayer = (wolf.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            if (angle < fovAngle / 2f)
            {
                if (!Physics.Linecast(transform.position, wolf.position, viewMask))
                {
                    OnTargetDetected();
                    return true;
                }
            }
        }
        return false;
    }

    // This method can be overridden by inherited classes for specific target detection logic
    protected virtual void OnTargetDetected()
    {
        Debug.Log("Detected Wolf!");
    }

    // Check if the target is obstructed by any obstacle
    protected virtual bool IsObstructed(Collider target)
    {
        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;

        if (
            Physics.Raycast(
                transform.position,
                directionToTarget,
                out RaycastHit hit,
                detectionDistance
            )
        )
        {
            if (hit.collider != target)
            {
                // If we hit something other than the target, it's obstructed
                return true;
            }
        }

        return false;
    }

    // Optional: Draw the FOV in the scene view for debugging
    private void OnDrawGizmos()
    {
        // Change Gizmo color based on whether a target has been detected
        if (targetDetected)
        {
            Gizmos.color = Color.red; // Red when a target is detected
        }
        else
        {
            Gizmos.color = Color.green; // Green otherwise
        }

        // Draw the FOV cone lines
        Vector3 fovLine1 =
            Quaternion.Euler(0, -fovAngle / 2, 0) * transform.forward * detectionDistance;
        Vector3 fovLine2 =
            Quaternion.Euler(0, fovAngle / 2, 0) * transform.forward * detectionDistance;

        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);

        Gizmos.DrawLine(
            transform.position,
            transform.position + transform.forward * detectionDistance
        );
    }
}
