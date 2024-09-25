using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public GameObject sheepPrefab;
    public int flockSize = 20;
    public float sheepSpeed = 2f;
    public float detectionRadius = 10f;

    public GameObject[] sheepArray;

    public Transform wolfTransform;

    void Start()
    {
        sheepArray = new GameObject[flockSize];

        for (int i = 0; i < flockSize; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 10f;
            sheepArray[i] = Instantiate(sheepPrefab, spawnPos, Quaternion.identity);
            sheepArray[i].GetComponent<SheepBehaviour>().flockManager = this;
        }
    }

    void Update()
    {
        foreach (GameObject sheep in sheepArray)
        {
            sheep.GetComponent<SheepBehaviour>().MoveSheep();
        }
    }
}
