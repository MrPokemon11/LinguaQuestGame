using TMPro;
using UnityEngine;

public class PrairieGameManager : MonoBehaviour
{
    public static PrairieGameManager Instance;
    public bool gameStarted = false;

    public GameObject sackPrefab;
    public Transform spawnPoint;

    public TextMeshProUGUI progressText;

    public float difficultyMultiplier = 1f;
    public float difficultyIncreaseRate = 0.05f; // increase every correct sack

    public int currentLane = 1; // 0 = top, 1 = middle, 2 = bottom
    public int correctCount = 0;
    public int winAmount = 5;

    private float spawnTimer = 0f;
    public float spawnInterval = 2f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!gameStarted)
            return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnSack();
            spawnTimer = spawnInterval;
        }
    }


    void SpawnSack()
    {
        Instantiate(sackPrefab, spawnPoint.position, Quaternion.identity);
    }

    public void RegisterCorrect()
    {
        correctCount++;
        UpdateUI();
        difficultyMultiplier += difficultyIncreaseRate;
        difficultyMultiplier = Mathf.Min(difficultyMultiplier, 2f);

        if (correctCount >= winAmount)
        {

            WinGame();
            SceneTracker.Instance.ReturnToPreviousScene(true);
        }
    }

    void UpdateUI()
    {
        progressText.text = correctCount + " / " + winAmount;
    }
    void WinGame()
    {
        gameStarted = false;

        PrairieProgress.Completed = true;

        SceneTracker.Instance.ReturnToPreviousScene(true);
    }
}