using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

public class QuizManager : MonoBehaviour
{
    private float startTime;
    private int clickedAnswers;

    public List<Category> selectedCategories = new List<Category>();
    int numberOfQuestionsToGenerate = 50;
    public List<Word2> words=new List<Word2>();

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI questionCounterText;
    public List<Button> answerButtons;
    public Button nextButton;

    public List<Question> questions;
    private int currentQuestionIndex = 0;
    public CategoryManager categoryManager;
    public List<Image> difficultyStars;
    public GameObject ResultScreen;
    int counter = 1;
    public WeaknessCalculator WeaknessCalculator;
    void Start()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);
        selectedCategories = categoryManager.selectedCategories;
        string jsonFilePath = "Assets/Resources/word_data.json";

        string jsonContent = File.ReadAllText(jsonFilePath);
        List<Root> categoryList = JsonConvert.DeserializeObject<List<Root>>(jsonContent);
        foreach (var item in categoryList)
        {
            words.AddRange(item.Words);
            foreach (var word in item.Words)
            {
                word.wordID = counter++;
            }
        }

        List <Word2> selectedWords = SelectRandomWords();

        questions = GenerateQuestions(selectedWords);
        for (int i = 0; i < answerButtons.Count; i++)
        {
            int index = i;
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
        }
        DisplayQuestion();
    }

    List<Word2> SelectRandomWords()
    {
        List<Word2> selectedWords = new List<Word2>();

        foreach (var category in selectedCategories)
        {
            var categoryWords = words.Where(word => word.Category == category.id).ToList();

            if (categoryWords.Count > 0)
            {
                selectedWords.AddRange(categoryWords);
            }

        }
        System.Random rng = new System.Random();
        selectedWords = selectedWords.OrderBy(_ => rng.Next()).ToList();

        return selectedWords;
    }


    List<Question> GenerateQuestions(List<Word2> selectedWords)
    {
        List<Question> generatedQuestions = new List<Question>();

        for (int i = 0; i < numberOfQuestionsToGenerate && i < selectedWords.Count; i++)
        {
            Word2 word = selectedWords[i];
            List<string> choices = GenerateChoices(word);

            Question question = new Question
            {
                WordId = word.wordID,
                TurkishWord = word.Turkish,
                Choices = choices,
                CorrectChoice = word.EnglishMeaning,
                CategoryId = word.Category,
                IsCorrect = false,
                TimeSpent = 0f,
                Difficulty = word.Difficulty
            };

            generatedQuestions.Add(question);
        }

        return generatedQuestions;
    }

    List<string> GenerateChoices(Word2 word)
    {
        HashSet<string> choices = new HashSet<string> { word.EnglishMeaning }; // Ensure no duplicates

        var otherWords = words.Where(w => w.Category == word.Category && w.Turkish != word.Turkish).ToList();
        var randomOtherWords = otherWords.OrderBy(x => Random.value).Take(3).Select(w => w.EnglishMeaning);

        foreach (var choice in randomOtherWords)
        {
            choices.Add(choice);
        }

        // If not enough choices, get more from different categories
        if (choices.Count < 4)
        {
            var extraChoices = words.Where(w => w.Category != word.Category)
                                    .OrderBy(x => Random.value)
                                    .Take(4 - choices.Count)
                                    .Select(w => w.EnglishMeaning);

            foreach (var choice in extraChoices)
            {
                choices.Add(choice);
            }
        }

        return choices.OrderBy(x => Random.value).ToList(); // Shuffle choices
    }

    void DisplayQuestion()
    {
        startTime = Time.time;
        clickedAnswers = 0;
        if (currentQuestionIndex < questions.Count)
        {
            Question currentQuestion = questions[currentQuestionIndex];

            questionText.text =currentQuestion.TurkishWord;
            questionCounterText.text = "Question: " + (currentQuestionIndex + 1) + "/" + questions.Count;
            for (int i = 0; i < answerButtons.Count; i++)
            {
                answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.Choices[i];
                RestButtonAppearance(i);
                
                
            }
            DisplayDifficultyStars(currentQuestion.Difficulty);

            EnableAnswerButtons(true);
        }
        else if (questions.Count > 0)
        {
            questionText.text = "Quiz Completed!";
            EnableAnswerButtons(false);
            nextButton.interactable = false;
        }
    }

    void DisplayDifficultyStars(int difficulty)
    {
        for (int i = 0; i < difficultyStars.Count; i++)
        {
            if (i < difficulty)
            {
                difficultyStars[i].color = new Color(1f, 1f, 1f, 1f);  
            }
            else
            {
                difficultyStars[i].color = new Color(1f, 1f, 1f, 0.3f); 
            }
        }
    }

    void EnableAnswerButtons(bool enable)
    {
        foreach (Button button in answerButtons)
        {
            button.interactable = enable;
        }
    }

    public void OnAnswerSelected(int choiceIndex)
    {
        /*
        Debug.Log(choiceIndex);
        Question currentQuestion = questions[currentQuestionIndex];
        currentQuestion.IsCorrect = currentQuestion.Choices[choiceIndex] == currentQuestion.CorrectChoice;
        UpdateSelectedButtonAppearance(choiceIndex);
        nextButton.interactable = true;
        */
        UpdateSelectedButtonAppearance(choiceIndex);
        clickedAnswers++;
        

        Question currentQuestion = questions[currentQuestionIndex];
        currentQuestion.IsCorrect = currentQuestion.Choices[choiceIndex] == currentQuestion.CorrectChoice;
    
        nextButton.interactable = true;
        
        currentQuestion.NumberOfClickedAnswers = clickedAnswers;
    }

    public void OnNextButtonClicked()
    {
        questions[currentQuestionIndex].TimeSpent = Time.time - startTime;
        currentQuestionIndex++;
        if (currentQuestionIndex >= questions.Count)
        {
            //SaveQuestionsToCSV("Assets/Resources/new/withoutWeakness.csv");
            ConvertQuestionsToDataEntries(questions);
            ResultScreen.SetActive(true);
            return;
        }
        DisplayQuestion();
    }
    void UpdateSelectedButtonAppearance(int selectedButtonIndex)
    {
        for (int i = 0; i < answerButtons.Count; i++)
        {

            Color bgColor = (i == selectedButtonIndex) ? new Color(132f / 255f, 32f / 255f, 253f / 255f, 1f) : Color.white;
            Color textColor = (i == selectedButtonIndex) ? Color.white : Color.black;

            var buttonImage = answerButtons[i].GetComponent<Image>();
            buttonImage.color = bgColor;

            var buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            buttonText.color = textColor;
        }
    }
    void ConvertQuestionsToDataEntries(List<Question> questions)
    {

        List<DataEntry> dataEntries= questions.Select(q => new DataEntry
        {
            WordID = q.WordId,
            CategoryID = q.CategoryId,
            TurkishWord = q.TurkishWord,
            CorrectAnswer = q.IsCorrect ? 1 : 0, 
            TimeSpent = q.TimeSpent,
            HesitationTimes = q.NumberOfClickedAnswers, 
            Difficulty = q.Difficulty,
            UserId = FirebaseManager.Instance.user.UserId,
            Weakness = 0  // This will be calculated later
        }).ToList();

        WeaknessCalculator.CalculateWeakness(dataEntries);
    }

    public void SaveQuestionsToCSV(string filePath)
    {
        FileMode fileMode = File.Exists(filePath) ? FileMode.Append : FileMode.CreateNew;

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            if (fileMode == FileMode.CreateNew)
            {
                writer.WriteLine("Word ID,Category ID,Turkish Word,Correct Answer, Time Spent,Hesitation Times, Difficulty,UserId");
            }

            foreach (Question question in questions)
            {
                writer.WriteLine(question.ToCSVString());
            }
        }




    }
    void RestButtonAppearance(int buttonIndex) {
        Color bgColor =  Color.white;
        Color textColor = Color.black;

        var buttonImage = answerButtons[buttonIndex].GetComponent<Image>();
        buttonImage.color = bgColor;
        var buttonText = answerButtons[buttonIndex].GetComponentInChildren<TextMeshProUGUI>();
        buttonText.color = textColor;
    }
}