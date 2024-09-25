using UnityEngine;

public class SheepCatcher : MonoBehaviour
{
    public int sheepCaught = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sheep"))
        {
            // Increment sheep counter and "catch" the sheep
            sheepCaught++;
            Destroy(other.gameObject); // Remove the sheep from the scene
            Debug.Log("Sheep caught! Total: " + sheepCaught);
        }
    }
}
