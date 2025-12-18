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
    public GameObject successPanel;
    public GameObject failPanel;
    public GameObject gameOverPanel;
    public GameObject HelpPanel;
    [Header("Start Messages")]
    public TMP_Text startPanelText;
    public List<string> startMessages = new List<string>()
    {
        "Welcome to the word lasso!",
        "Click to shoot the hook and grab words.",
        "Collect them in the correct order to form the sentence.",
        "Right-click to put back the last word.",
        "Get all sentence correct to help lovely squirels gather nuts.",
        "Good luck!"
    };
    public float startMessageDuration = 2f;

    [Header("Gameplay Data")]
    public List<WordTargetController> activeWords = new List<WordTargetController>();
    private List<string> collectedWords = new List<string>();
    private Stack<WordTargetController> collectedWordStack = new Stack<WordTargetController>();
    public bool paused = false;


    void Start()
    {
        // Show start UI first
        ShowOnly(startPanel);

        // continue after 2 seconds
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        ShowOnly(startPanel);
        yield return StartCoroutine(ShowStartMessages());

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

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            PutWordBack();
        }
    }

    public void SpawnWords(WordOrderQuestion question)
    {
        foreach (var w in activeWords)
            if (w != null) Destroy(w.gameObject);

        activeWords.Clear();
        collectedWords.Clear();
        collectedWordStack.Clear();

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
        float spacing = (halfSize.x * 2f) / shuffledParts.Count * 1.15f;

        // Adjust the spawn position of the first word
        float offset = 1f; // Adjust this value to move the first word more left
        for (int i = 0; i < shuffledParts.Count; i++)
        {
            Vector2 spawnPos = new Vector2(center.x - halfSize.x + spacing / 2f + i * spacing - offset, center.y);
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
        collectedWordStack.Push(word);

        UpdateSentenceText(string.Join(" ", collectedWords));
        UpdateCollectedWordsUI();

        FindFirstObjectByType<GunScript>().ReleaseHook();

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


    public void PutWordBack()
    {
        if (collectedWordStack.Count == 0)
            return;

        var lastWord = collectedWordStack.Pop();

        if (collectedWords.Count > 0)
            collectedWords.RemoveAt(collectedWords.Count - 1);

        lastWord.ResetWord();

        if (!activeWords.Contains(lastWord))
            activeWords.Add(lastWord);

        UpdateSentenceText(string.Join(" ", collectedWords));
        UpdateCollectedWordsUI();
        UpdateFeedback("");

        var gun = FindFirstObjectByType<GunScript>();
        if (gun != null)
            gun.ReleaseHook();
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
            StartCoroutine(gameOver());
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

    IEnumerator ShowStartMessages()
    {
        if (startPanel == null)
        {
            yield return new WaitForSeconds(startMessageDuration);
            yield break;
        }

        startPanel.SetActive(true);

        if (startPanelText != null && startMessages != null && startMessages.Count > 0)
        {
            foreach (var msg in startMessages)
            {
                startPanelText.text = msg;
                float duration = Mathf.Max(0.5f, startMessageDuration);
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    if (Input.GetKeyDown(KeyCode.V))
                    {
                        break;
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(startMessageDuration);
        }
    }

    IEnumerator gameOver()
    {
        ShowOnly(gameOverPanel);
        int ePressCount = 0;
        while (ePressCount < 1)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ePressCount++;
            }
            string _goMarker = "|goStart:";
            float _timeoutSeconds = 3f;

            if (gameOverPanel != null)
            {
                if (!gameOverPanel.name.Contains(_goMarker))
                {
                    gameOverPanel.name += _goMarker + Time.realtimeSinceStartup.ToString("F3");
                }
                else
                {
                    int _idx = gameOverPanel.name.IndexOf(_goMarker);
                    string _timeStr = gameOverPanel.name.Substring(_idx + _goMarker.Length);
                    if (float.TryParse(_timeStr, out float _start))
                    {
                        if (Time.realtimeSinceStartup - _start >= _timeoutSeconds)
                        {
                            // clean up marker and break to return
                            gameOverPanel.name = gameOverPanel.name.Substring(0, _idx);
                            break;
                        }
                    }
                }
            }
            yield return null;
        }

        SceneTracker.Instance.ReturnToPreviousScene(true);
    }

    public void ShowHelpPanel()
    {
        if (HelpPanel != null)
        {
            HelpPanel.SetActive(true);
            paused = true;
            Time.timeScale = 0f;
        }
    }

    public void HideHelpPanel()
    {
        if (HelpPanel != null)
        {
            HelpPanel.SetActive(false);
            paused = false;
            Time.timeScale = 1f;
        }
    }
}