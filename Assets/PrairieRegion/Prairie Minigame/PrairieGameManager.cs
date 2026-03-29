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
    public float difficultyIncreaseRate = 0.05f;

    public int currentLane = 1; // 0 = top, 1 = middle, 2 = bottom
    public int correctCount = 0;
    public int winAmount = 5;

    private float spawnTimer = 0f;
    public float spawnInterval = 2f;

    [Header("Win Settings")]
    public AudioSource audioSource;
    public AudioClip winSound;
    public float winDelay = 1f;

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

        // play win sound
        if (audioSource != null && winSound != null)
        {
            audioSource.PlayOneShot(winSound);
        }

        // delay before returning
        Invoke(nameof(ReturnToPrairie), winDelay);
    }

    void ReturnToPrairie()
    {
        SceneTracker.Instance.ReturnToPreviousScene(true);
    }
}