using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordLassoManager : MonoBehaviour
{
    [Header("Setup")]
    public Transform spawnArea;              // The WordSpawnArea in your scene
    public GameObject wordPrefab;            // Prefab for each word target
    public WordOrderQuestion currentQuestion;

    [Header("UI Feedback")]
    public TMP_Text currentSentenceText;     // Optional TMP at bottom to show progress
    public TMP_Text feedbackText;            // Optional TMP for “Correct!” / “Try again!”

    [Header("Gameplay Data")]
    public List<WordTargetController> activeWords = new List<WordTargetController>();
    private List<string> collectedWords = new List<string>();

    void Start()
    {
        if (currentQuestion != null)
        {
            SpawnWords(currentQuestion);
        }
    }

    // ---------------------------------------------------------------
    // Spawning words
    // ---------------------------------------------------------------
    public void SpawnWords(WordOrderQuestion question)
    {
        // Destroy old ones
        foreach (var w in activeWords)
        {
            if (w != null) Destroy(w.gameObject);
        }
        activeWords.Clear();
        collectedWords.Clear();

        UpdateSentenceText("");
        UpdateFeedback("");

        currentQuestion = question;

        // Shuffle the word parts randomly
        List<string> shuffledParts = new List<string>(question.wordParts);
        for (int i = 0; i < shuffledParts.Count; i++)
        {
            int randIndex = Random.Range(i, shuffledParts.Count);
            (shuffledParts[i], shuffledParts[randIndex]) = (shuffledParts[randIndex], shuffledParts[i]);
        }

        // Spawn words horizontally across spawn area
        Vector2 center = spawnArea.position;
        Vector2 halfSize = spawnArea.localScale / 2f;
        float spacing = (halfSize.x * 2f) / shuffledParts.Count;

        for (int i = 0; i < shuffledParts.Count; i++)
        {
            Vector2 spawnPos = new Vector2(center.x - halfSize.x + spacing / 2f + i * spacing, center.y);
            GameObject newWord = Instantiate(wordPrefab, spawnPos, Quaternion.identity);
            var controller = newWord.GetComponent<WordTargetController>();
            controller.SetWord(shuffledParts[i], this);
            activeWords.Add(controller);
        }
    }

    // ---------------------------------------------------------------
    // Called when a word reaches the catch zone
    // ---------------------------------------------------------------
    public void OnWordCollected(WordTargetController word)
    {
        string w = word.GetWord();
        collectedWords.Add(w);
        activeWords.Remove(word);

        UpdateSentenceText(string.Join(" ", collectedWords));

        FindObjectOfType<GunScript>().ReleaseHook();

        // Check correctness after each collection
        if (collectedWords.Count == currentQuestion.wordParts.Length)
        {
            if (IsCorrectOrder())
            {
                UpdateFeedback("Correct!");
                Debug.Log("Sentence complete!");
            }
            else
            {
                UpdateFeedback("Try again!");
                StartCoroutine(ResetAfterDelay(1.5f));
            }
        }
    }

    // ---------------------------------------------------------------
    // Check if order matches correct sequence
    // ---------------------------------------------------------------
    bool IsCorrectOrder()
    {
        // Build what the correct answer should be
        List<string> correctSequence = new List<string>();
        foreach (int idx in currentQuestion.correctOrderIndices)
        {
            correctSequence.Add(currentQuestion.wordParts[idx]);
        }

        if (correctSequence.Count != collectedWords.Count)
            return false;

        for (int i = 0; i < correctSequence.Count; i++)
        {
            if (correctSequence[i] != collectedWords[i])
                return false;
        }

        return true;
    }

    IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnWords(currentQuestion);
    }

    void UpdateSentenceText(string txt)
    {
        if (currentSentenceText != null)
            currentSentenceText.text = txt;
    }

    void UpdateFeedback(string txt)
    {
        if (feedbackText != null)
            feedbackText.text = txt;
    }
}