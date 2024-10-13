using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SheepCatcher : MonoBehaviour
{
    public float catchRange = 7f;
    public float catchTime = 0.5f;
    public LayerMask sheepLayer;
    public float catchAngle = 180f;

    [Header("UI")]
    public TextMeshProUGUI hintText; // Reference to the UI Text component
    public string hintMessage = "Press<space=0.2em><size=120%><b><color=\"green\">E</b></color></size><space=0.2em>to catch the sheep";

    private float catchProgress = 0f;
    private GameObject targetSheep;
    private bool isCatching = false;

    void Update()
    {
        Collider[] nearbySheep = Physics.OverlapSphere(transform.position, catchRange, sheepLayer);
        targetSheep = GetClosestSheepInArc(nearbySheep);

        if (targetSheep != null)
        {
            Debug.Log("SheepFound");
            ShowHint(true);

            if (Input.GetKey(KeyCode.E))
            {
                if (!isCatching)
                {
                    isCatching = true;
                    catchProgress = 0f;
                }

                catchProgress += Time.deltaTime;

                if (catchProgress >= catchTime)
                {
                    CatchSheep(targetSheep);
                }
            }
            else
            {
                isCatching = false;
                catchProgress = 0f;
            }
        }
        else
        {
            ShowHint(false);
            isCatching = false;
            catchProgress = 0f;
        }
    }

    private GameObject GetClosestSheepInArc(Collider[] sheepColliders)
    {
        GameObject closestSheep = null;
        float closestDistance = float.MaxValue;

        foreach (Collider sheepCollider in sheepColliders)
        {
            Vector3 directionToSheep = sheepCollider.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, directionToSheep);

            // Check if the sheep is within the forward arc
            if (angle <= catchAngle / 2)
            {
                float distance = directionToSheep.magnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSheep = sheepCollider.gameObject;
                }
            }
        }

        return closestSheep;
    }

    private void CatchSheep(GameObject sheep)
    {
        Debug.Log("Sheep caught!");
        // Add your code here to handle the caught sheep
        // For example, you might want to disable the sheep's script, change its appearance, or add it to the player's inventory

        // Example: Disable the sheep's behavior script
        SheepBehaviour sheepBehavior = sheep.GetComponent<SheepBehaviour>();
        if (sheepBehavior != null)
        {
            sheepBehavior.enabled = false;
        }

        // Example: Change the sheep's color to indicate it's caught
        Renderer sheepRenderer = sheep.GetComponent<Renderer>();
        if (sheepRenderer != null)
        {
            sheepRenderer.material.color = Color.blue;
        }

        // Reset catching state
        isCatching = false;
        catchProgress = 0f;
        targetSheep = null;
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the catch range and arc in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchRange);

        Vector3 forward = transform.forward;
        Vector3 right = Quaternion.Euler(0, catchAngle / 2, 0) * forward;
        Vector3 left = Quaternion.Euler(0, -catchAngle / 2, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + right * catchRange);
        Gizmos.DrawLine(transform.position, transform.position + left * catchRange);
    }

     private void ShowHint(bool show)
    {
        if (hintText != null)
        {
            hintText.gameObject.SetActive(show);
            if (show)
            {
                hintText.text = hintMessage;
            }
        }
    }
}