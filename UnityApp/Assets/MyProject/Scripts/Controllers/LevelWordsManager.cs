using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LevelWordsManager : MonoBehaviour
{
    public TextMeshProUGUI turkishWordText; // UI Text for Turkish word
    public TextMeshProUGUI englishWordText; // UI Text for English translation
    public TextMeshProUGUI progressText;    // UI Text for progress (e.g., "Word 1/10")
    public Button nextButton;    // UI Button for navigating words

    public GameObject lessonsPage;
    public List<Word2> levelWords = new List<Word2>(); // List of words
    public int currentWordIndex = 0;
    public int levelNumber; // Current level number
    private FirebaseManager firebaseManager; // Reference to FirebaseManager

    private void Start()
    {
        
        firebaseManager = FirebaseManager.Instance;
        //InitializeLevelWords();
        UpdateWordUI();

        // Display the first word
        nextButton.onClick.AddListener(() => OnNextButtonClicked());

    }
    public void Setup()
    {
        UpdateWordUI();

    }

    //private void InitializeLevelWords()
    //{
    //    // TODO: Replace with your word-fetching logic (e.g., Firebase or predefined list)
    //    levelWords = new List<(string, string)>
    //    {
    //        ("Merhaba", "Hello"),
    //        ("Elma", "Apple"),
    //        ("Kitap", "Book"),
    //        ("Masa", "Table"),
    //        ("Kedi", "Cat")
    //    };

    //    // Assume level number is passed or set externally (e.g., via GameManager)
    //    //levelNumber = PlayerPrefs.GetInt("SelectedLevel", 1); // Example for storing selected level
    //}

    private void UpdateWordUI()
    {
        if (currentWordIndex < levelWords.Count)
        {
            // Update the UI with the current word
            var word = levelWords[currentWordIndex];
            turkishWordText.text = word.Turkish;
            englishWordText.text = word.EnglishMeaning;
            progressText.text = $"Word {currentWordIndex + 1}/{levelWords.Count}";
            nextButton.GetComponentInChildren<TextMeshProUGUI>().text = "Next";
        }
        else
        {
            // All words learned, change button to "Done"
            turkishWordText.text = "";
            englishWordText.text = "You have completed this level!";
            progressText.text = $"Word {levelWords.Count}/{levelWords.Count}";
            nextButton.GetComponentInChildren<TextMeshProUGUI>().text = "Done";
        }
    }

    public void OnNextButtonClicked()
    {
        if (currentWordIndex < levelWords.Count)
        {
            // Save progress in Firebase
            firebaseManager.SaveLessonProgress(firebaseManager.user.UserId, levelNumber, currentWordIndex + 1, levelWords.Count);
            GameManager.Instance.AddGemsToScore(levelNumber*2);
            // Advance to the next word
            currentWordIndex++;

            // Update the UI
            UpdateWordUI();
        }
        else
        {
            // Return to the lessons page
            lessonsPage.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}
