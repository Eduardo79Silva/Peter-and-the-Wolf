using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public int sheepCaptureCount = 0;
    public float gameTime = 300f; // Total game time in seconds (e.g., 5 minutes)
    public bool isGameActive = false;

    [Header("UI References")]
    public Text sheepCountText;
    public Text timerText;
    public GameObject gameOverPanel;
    public Text gameOverText;

    [Header("Spawn Settings")]
    public Vector3 spawnAreaCenter = Vector3.zero;
    public Vector3 spawnAreaSize = new(10, 0, 10);

    private float currentGameTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    private void Update()
    {
        if (isGameActive)
        {
            UpdateGameTime();
            CheckGameOver();
        }
    }

    public void InitializeGame()
    {
        sheepCaptureCount = 0;
        currentGameTime = gameTime;
        isGameActive = true;
        UpdateUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Initialize FlockManager and spawn sheep
        if (FlockManager.FM != null)
        {
            FlockManager.FM.transform.position = spawnAreaCenter;
            FlockManager.FM.fleeLimits = spawnAreaSize / 2f; // Assuming fleeLimits is half the spawn area size
        }
        else
        {
            Debug.LogError("FlockManager (FM) is not set up in the scene!");
        }
    }

    private void SpawnSheep()
    {
        if (FlockManager.FM != null)
        {
            // Clear existing sheep if any
            if (FlockManager.FM.allSheep != null)
            {
                foreach (var sheep in FlockManager.FM.allSheep)
                {
                    if (sheep != null) Destroy(sheep);
                }
            }

            // Initialize new array for sheep
            FlockManager.FM.allSheep = new GameObject[FlockManager.FM.flockSize];

            // Spawn new sheep
            for (int i = 0; i < FlockManager.FM.flockSize; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(-FlockManager.FM.fleeLimits.x, FlockManager.FM.fleeLimits.x),
                    1, // Assuming y = 1 is ground level
                    Random.Range(-FlockManager.FM.fleeLimits.z, FlockManager.FM.fleeLimits.z)
                );
                Vector3 spawnPos = FlockManager.FM.transform.position + randomPos;
                FlockManager.FM.allSheep[i] = Instantiate(FlockManager.FM.sheepPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    public void CaptureSheep()
    {
        sheepCaptureCount++;
        UpdateUI();
    }

    private void UpdateGameTime()
    {
        currentGameTime -= Time.deltaTime;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (sheepCountText != null && FlockManager.FM != null)
            sheepCountText.text = $"Sheep Captured: {sheepCaptureCount} / {FlockManager.FM.flockSize}";

        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(currentGameTime)}s";
    }

    private void CheckGameOver()
    {
        if (FlockManager.FM != null && (sheepCaptureCount >= FlockManager.FM.flockSize || currentGameTime <= 0))
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameActive = false;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null && FlockManager.FM != null)
            {
                string result = sheepCaptureCount >= FlockManager.FM.flockSize ? "You Win!" : "Time's Up!";
                gameOverText.text = $"{result}\nSheep Captured: {sheepCaptureCount} / {FlockManager.FM.flockSize}";
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}