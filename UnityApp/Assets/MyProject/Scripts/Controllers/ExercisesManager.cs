using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.IO;

public class ExercisesManager : MonoBehaviour
{
    public GameObject exercizePrefab;  // Combined prefab for both button and progress text
    public Transform exercizeContentPanel;  // The parent panel where the exercizes will be placed
    private FirebaseManager firebaseManager;

    public Dictionary<int, int> exercizeProgress = new Dictionary<int, int>(); // Store progress here
    public List<GameObject> exercizeItems = new List<GameObject>(); // List to store dynamically created exercize items
    public Dictionary<int, List<Word2>> levelWords = new Dictionary<int, List<Word2>>(); // Words assigned to each level
    public GameObject exercizesDetailsPage;
    public GameObject loading;
    private Color currentColor;
    private void Start()
    {
        levelWords=GameManager.Instance.levelWords;
        GenerateExercizeUI(Constants.levelsCount);
    }
    private void OnEnable()
    {
        loading.SetActive(true);
        firebaseManager = FirebaseManager.Instance;
        firebaseManager.FetchAllExercizesMarks(firebaseManager.user.UserId, OnProgressFetched);

        Invoke("UpdateexercizesProgress",.5f);
    }
   
    private void OnProgressFetched(Dictionary<int, int> progress)
    {
        exercizeProgress = progress;
        
        
    }
    private void UpdateexercizesProgress()
    {
       
        for (int i = 1; i < Constants.levelsCount; i++)
        {
            int zeroBasedIndex = i - 1;
            Button levelButton = exercizeItems[zeroBasedIndex].GetComponentInChildren<Button>();
            currentColor = levelButton.GetComponent<Image>().color;
            if (exercizeProgress.ContainsKey(i))
            {
                TextMeshProUGUI progressText = exercizeItems[zeroBasedIndex].GetComponentsInChildren<TextMeshProUGUI>()[1];
                progressText.text = $"{exercizeProgress[i]}/{levelWords[i].Count}";
                
                levelButton.GetComponentInChildren<TextMeshProUGUI>().text = "Remake";
                if(exercizeProgress[i]/ levelWords[i].Count>=0.5f)
                    levelButton.GetComponent<Image>().color = Color.green;
                else
                    levelButton.GetComponent<Image>().color = Color.red;

            }
            else
            {
                levelButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
                levelButton.GetComponent<Image>().color = currentColor;

            }
        }
        loading.SetActive(false);

    }


    private void GenerateExercizeUI(int levelCount)
    {
        for (int i = 0; i < levelCount; i++)
        {
            GameObject exercizeItem = Instantiate(exercizePrefab, exercizeContentPanel);
            exercizeItems.Add(exercizeItem);
            LevelItemController levelItem= exercizeItem.GetComponent<LevelItemController>();
           
            int level = i + 1;  // Levels start from 1
            levelItem.LevelText.text = $"Exercise {level}";

            // Log to ensure the level number and progress are being set correctly
            int progress = exercizeProgress.ContainsKey(level) ? exercizeProgress[level] : 0;
            levelItem.progressText.text = $"{progress}/{levelWords[level].Count}";
            levelItem.learnBTN.onClick.AddListener(() => OnExercizeButtonClicked(level,progress));
            
        }


    }




    public void OnExercizeButtonClicked(int level,int progress)
    {
        exercizesDetailsPage.SetActive(true);
        exercizesDetailsPage.GetComponent<QuizAppManager>().currentQuestionIndex = progress;
        exercizesDetailsPage.GetComponent<QuizAppManager>().level = level;
        exercizesDetailsPage.GetComponent<QuizAppManager>().words = levelWords[level];
        exercizesDetailsPage.GetComponent<QuizAppManager>().Setup();
        
        this.gameObject.SetActive(false);
    }


}
