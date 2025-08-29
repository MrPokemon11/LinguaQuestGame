//using UnityEngine;
//using UnityEngine.UI;

//public class QuizManager : MonoBehaviour
//{
//    public GameObject quizPanel; // UI panel
//    public Text questionText;
//    public Button[] answerButtons;
//    // public SpikeController spikeController;

//    private int currentLeverID;
//    private int[] correctAnswers = new int[4]; // track correct answers per lever

//    public void StartQuiz(int leverID)
//    {
//        currentLeverID = leverID;
//        quizPanel.SetActive(true);
//        ShowQuestion(leverID);
//    }

//    void ShowQuestion(int leverID)
//    {
//        // For simplicity, example questions. Can be expanded or read from ScriptableObjects
//        string[] questions = {
//            "What is 'Hello' in Japanese?",
//            "What is 'Thank you' in Japanese?",
//            "What is 'Goodbye' in Japanese?",
//            "What is 'Yes' in Japanese?"
//        };
//        questionText.text = questions[leverID];

//        // Assign answers dynamically or statically
//        // Example: correct answer = first button
//        for (int i = 0; i < answerButtons.Length; i++)
//        {
//            int index = i; // capture variable
//            answerButtons[i].onClick.RemoveAllListeners();
//            answerButtons[i].onClick.AddListener(() => Answer(index == 0));
//        }
//    }

//    void Answer(bool isCorrect)
//    {
//        if (isCorrect)
//        {
//            correctAnswers[currentLeverID] = 1;
//            spikeController.LowerSpike(currentLeverID);
//        }
//        quizPanel.SetActive(false);

//        // Optional: check if all answers are correct
//        if (System.Array.TrueForAll(correctAnswers, x => x == 1))
//        {
//            spikeController.UnlockRegion();
//        }
//    }
//}
