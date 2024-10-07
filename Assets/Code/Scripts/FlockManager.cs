using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public static FlockManager FM;
    public GameObject sheepPrefab;
    public GameObject[] allSheep;

    public int flockSize = 20;
    public Vector3 fleeLimits = new(5, 5, 5);
    public Vector3 goalPos = Vector3.zero;

    [Header("Fish Settings")]
    [Range(0.0f, 5.0f)]
    public float minSpeed;

    [Range(0.0f, 5.0f)]
    public float maxSpeed;

    [Range(1.0f, 10.0f)]
    public float neighborDistance;

    [Range(1.0f, 5.0f)]
    public float rotationSpeed;

    void Start()
    {
        FM = this;
        allSheep = new GameObject[flockSize];
        for (int i = 0; i < flockSize; i++)
        {
            Vector3 pos =
                this.transform.position
                + new Vector3(
                    Random.Range(-fleeLimits.x, fleeLimits.x),
                    1,
                    Random.Range(-fleeLimits.z, fleeLimits.z)
                );

            allSheep[i] = Instantiate(sheepPrefab, pos, Quaternion.identity);
        }
        goalPos = this.transform.position;
    }

    void Update()
    {
        if (UnityEngine.Random.Range(0, 100) < 1)
        {
            goalPos = new Vector3(
                    Random.Range(-fleeLimits.x, fleeLimits.x),
                    1,
                    Random.Range(-fleeLimits.z, fleeLimits.z)
                );
        }
    }
}
