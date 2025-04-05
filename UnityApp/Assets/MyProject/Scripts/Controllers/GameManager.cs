using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int currentScore;
    private string csvFilePath = "Assets/Resources/new/main_dataset2.csv";
    string categories = "Assets/Resources/new/categories.csv";
    public List<Category> categoriesList = new List<Category>();
    public List<CategoryWeakness> weaknessCategories = new List<CategoryWeakness>();
    public Dictionary<int, List<Word2>> levelWords = new Dictionary<int, List<Word2>>(); // Words assigned to each level
    public WeaknessCalculator weaknessCalculator;
    public APIManager apiManager;
    public List<Word2> allWords=new List<Word2>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadScore();
        //GenerateLessonUI(levelCount);
        allWords = LoadWordsFromCSV(csvFilePath);

        // Sort the words by difficulty (ascending order)
        List<Word2> sortedWords = allWords.OrderBy(w => w.Difficulty).ToList();

        // Distribute sorted words to levels
        DistributeWordsToLevels(sortedWords);
        //Get cateogies from csv file
        GetCategories();
        weaknessCalculator=GetComponent<WeaknessCalculator>();
        apiManager = GetComponent<APIManager>();
        FirebaseManager.Instance.LoadCategoryWeakness(FirebaseManager.Instance.user.UserId,null);
    }
    void GetCategories() {
        // Read CSV file and parse categories
        using (var reader = new StreamReader(categories))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                // Assuming the CSV file has columns: id,name,image_path
                if (values.Length >= 3)
                {
                    int id;
                    if (int.TryParse(values[0], out id))
                    {
                        Category category = new Category
                        {
                            id = id,
                            name = values[1],
                            image = values[2]
                        };
                        categoriesList.Add(category);
                    }
                }
            }
        }
    }
    public string GetFormattedScore(int score)
    {
        if (score >= 1_000_000_000)
            return (score / 1_000_000_000f).ToString("0.##") + "b"; // Billions
        else if (score >= 1_000_000)
            return (score / 1_000_000f).ToString("0.##") + "m"; // Millions
        else if (score >= 1_000)
            return (score / 1_000f).ToString("0.##") + "k"; // Thousands
        else
            return score.ToString(); // Less than a thousand
    }
    private List<Word2> LoadWordsFromCSV(string path)
    {
        List<Word2> words = new List<Word2>();

        // Read the CSV file
        string[] lines = File.ReadAllLines(path);
        // Skip the header line (if there's one)
        for (int i = 1; i < 1607; i++)
        {
            //Debug.Log(lines[i]);

            string[] columns = lines[i].Split(';');
            Word2 word = new Word2
            {
                wordID = int.Parse(columns[0]),
                Turkish = columns[1],
                EnglishMeaning = columns[2],
                Category = int.Parse(columns[3]),
                Difficulty = int.Parse(columns[4])
            };
            words.Add(word);
        }

        return words;
    }
    private void DistributeWordsToLevels(List<Word2> sortedWords)
    {
        int wordsPerLevel = Mathf.CeilToInt(sortedWords.Count / (float)Constants.levelsCount);

        for (int i = 0; i < Constants.levelsCount; i++)
        {
            int startIndex = i * wordsPerLevel;
            int endIndex = Mathf.Min(startIndex + wordsPerLevel, sortedWords.Count);

            // Get the words for the current level
            List<Word2> levelWordsForCurrentLevel = sortedWords.GetRange(startIndex, endIndex - startIndex);
            levelWords[i + 1] = levelWordsForCurrentLevel;

            //Debug.Log($"Level {i + 1}: {levelWordsForCurrentLevel.Count} words");
        }
    }
    public void AddGemsToScore(int gems)
    {
        currentScore += gems;
        FirebaseManager.Instance.AddScore(gems);
        //Debug.Log($"Added {gems} gems. New score: {currentScore}");
    }

    public void LoadScore()
    {
        if (FirebaseManager.Instance.currentUserProfile != null)
        {
            currentScore = FirebaseManager.Instance.currentUserProfile.Score;
            Debug.Log($"Loaded score: {currentScore}");
        }
        else
        {
            Debug.LogError("Unable to load score. User profile is null.");
        }
    }
}
