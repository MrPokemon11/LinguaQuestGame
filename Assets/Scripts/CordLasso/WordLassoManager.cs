using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordLassoManager : MonoBehaviour
{
    [Header("Setup")]
    public Transform spawnArea;
    public GameObject wordPrefab;

    public List<WordOrderQuestion> questions;
    public int currentQuestionIndex = 0;

    [Header("UI Feedback")]
    public TMP_Text currentSentenceText;
    public TMP_Text feedbackText;
    public TMP_Text collectedWordsText;         

    [Header("UI Panels")]
    public GameObject startPanel;               
    public GameObject instructionPanel; 
    public GameObject successPanel;
    public GameObject failPanel;
    public GameObject gameOverPanel;

    [Header("Gameplay Data")]
    public List<WordTargetController> activeWords = new List<WordTargetController>();
    private List<string> collectedWords = new List<string>();


    void Start()
    {
        // Show start UI first
        ShowOnly(startPanel);

        // continue after 2 seconds
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        yield return new WaitForSeconds(2f);

        // show instructions for 2 seconds
        ShowOnly(instructionPanel);
        yield return new WaitForSeconds(2f);

        HideAllPanels();

        if (questions != null && questions.Count > 0)
        {
            currentQuestionIndex = 0;
            SpawnWords(questions[currentQuestionIndex]);
        }
        else
        {
            Debug.LogError("NO QUESTIONS ASSIGNED");
        }
    }

    public void SpawnWords(WordOrderQuestion question)
    {
        foreach (var w in activeWords)
            if (w != null) Destroy(w.gameObject);

        activeWords.Clear();
        collectedWords.Clear();

        UpdateSentenceText("");
        UpdateFeedback("");
        UpdateCollectedWordsUI();

        // shuffle logic
        List<string> shuffledParts = new List<string>(question.wordParts);
        for (int i = 0; i < shuffledParts.Count; i++)
        {
            int randIndex = Random.Range(i, shuffledParts.Count);
            (shuffledParts[i], shuffledParts[randIndex]) =
                (shuffledParts[randIndex], shuffledParts[i]);
        }

        // spawn words horizontally
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


    public void OnWordCollected(WordTargetController word)
    {
        string w = word.GetWord();
        collectedWords.Add(w);
        activeWords.Remove(word);

        UpdateSentenceText(string.Join(" ", collectedWords));
        UpdateCollectedWordsUI();

        FindObjectOfType<GunScript>().ReleaseHook();

        // when all the words are collected
        if (collectedWords.Count == questions[currentQuestionIndex].wordParts.Length)
        {
            if (IsCorrectOrder())
            {
                UpdateFeedback("Correct!");
                ShowOnly(successPanel);

                StartCoroutine(LoadNextQuestionAfterDelay(1.0f));
            }
            else
            {
                UpdateFeedback("Try again!");
                ShowOnly(failPanel);

                StartCoroutine(ResetSameQuestionAfterDelay(1.5f));
            }
        }
    }


    bool IsCorrectOrder()
    {
        WordOrderQuestion q = questions[currentQuestionIndex];

        List<string> correctSequence = new List<string>();
        foreach (int idx in q.correctOrderIndices)
            correctSequence.Add(q.wordParts[idx]);

        if (correctSequence.Count != collectedWords.Count)
            return false;

        for (int i = 0; i < correctSequence.Count; i++)
            if (correctSequence[i] != collectedWords[i])
                return false;

        return true;
    }


    IEnumerator ResetSameQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        HideAllPanels();
        SpawnWords(questions[currentQuestionIndex]);
    }

    IEnumerator LoadNextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        currentQuestionIndex++;

        if (currentQuestionIndex >= questions.Count)
        {
            ShowOnly(gameOverPanel);
            yield break;
        }

        HideAllPanels();
        SpawnWords(questions[currentQuestionIndex]);
    }

    // UI functions
    void ShowOnly(GameObject panel)
    {
        HideAllPanels();
        if (panel != null) panel.SetActive(true);
    }

    void HideAllPanels()
    {
        if (startPanel) startPanel.SetActive(false);
        if (instructionPanel) instructionPanel.SetActive(false);
        if (successPanel) successPanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
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

    void UpdateCollectedWordsUI()
    {
        if (collectedWordsText != null)
            collectedWordsText.text = "Collected: " + string.Join(" ", collectedWords);
    }
}