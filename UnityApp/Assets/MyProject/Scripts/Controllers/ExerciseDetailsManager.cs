using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ExerciseDetailsManager : MonoBehaviour
{
    public TextMeshProUGUI turkishWord, questionCounter;
    public Button[] answersButtons;
    public Button NextButton;
    public List<Word2> words = new List<Word2>();
    public GameObject exercisesPage;
    private List<Word2> shuffledWords;
    private int currentQuestionIndex = 0;
    public int exerciseNumber = 0;
    private int correctAnswers=0;

    private void Start()
    {
        foreach (Word2 word in words)
        {
        Debug.Log(word.Turkish);
        }
        PrepareQuestions();
        DisplayCurrentQuestion();
        NextButton.onClick.AddListener(OnNextButtonClicked);
    }

    private void PrepareQuestions()
    {
        // Shuffle the words to ensure randomness
        shuffledWords = words.OrderBy(w => Random.value).ToList();
    }

    private void DisplayCurrentQuestion()
    {
        if (currentQuestionIndex >= shuffledWords.Count)
        {
            Debug.LogWarning("No more questions to display.");
            return;
        }

        // Current question
        Word2 currentWord = shuffledWords[currentQuestionIndex];

        // Set Turkish word text
        turkishWord.text = currentWord.Turkish;

        // Update question counter
        questionCounter.text = $"Question {currentQuestionIndex + 1} / {shuffledWords.Count}";

        // Generate answer options
        List<string> allAnswers = words
            .Where(w => w.wordID != currentWord.wordID) // Exclude the correct answer
            .Select(w => w.EnglishMeaning)
            .OrderBy(w => Random.value)
            .Take(3)
            .ToList();

        allAnswers.Add(currentWord.EnglishMeaning); // Add the correct answer
        allAnswers = allAnswers.OrderBy(a => Random.value).ToList(); // Shuffle answers

        // Assign answers to buttons
        for (int i = 0; i < answersButtons.Length; i++)
        {
            answersButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = allAnswers[i];
            bool isCorrect = allAnswers[i] == currentWord.EnglishMeaning;

            answersButtons[i].onClick.RemoveAllListeners();
            answersButtons[i].onClick.AddListener(() => OnAnswerSelected(isCorrect));
        }

        // Update "Next" button text
        if (currentQuestionIndex == shuffledWords.Count - 1)
        {
            NextButton.GetComponentInChildren<TextMeshProUGUI>().text = "Submit";
        }
        else
        {
            NextButton.GetComponentInChildren<TextMeshProUGUI>().text = "Next";
        }
    }

    private void OnAnswerSelected(bool isCorrect)
    {
        if (isCorrect)
        {
            Debug.Log("Correct answer!");
            FirebaseManager.Instance.SaveExerciseProgress(FirebaseManager.Instance.user.UserId, exerciseNumber, ++correctAnswers);

        }
        else
        {
            FirebaseManager.Instance.SaveExerciseProgress(FirebaseManager.Instance.user.UserId, exerciseNumber, correctAnswers);
            Debug.Log("Wrong answer.");
        }

        // Disable buttons to prevent further selections
        foreach (Button button in answersButtons)
        {
            button.interactable = false;
        }
    }

    private void OnNextButtonClicked()
    {
        foreach (Button button in answersButtons)
        {
            button.interactable = true;
        }
        if (currentQuestionIndex < shuffledWords.Count - 1)
        {
            currentQuestionIndex++;
            DisplayCurrentQuestion();
        }
        else
        {
            Debug.Log("Quiz completed!");
            // Handle quiz submission logic here
            exercisesPage.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}
