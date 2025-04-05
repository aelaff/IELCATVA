using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.IO;
using static FirebaseManager;

public class LessonManager : MonoBehaviour
{
    public GameObject lessonPrefab;  // Combined prefab for both button and progress text
    public Transform lessonContentPanel;  // The parent panel where the lessons will be placed
    private FirebaseManager firebaseManager;

    public Dictionary<int, LessonProgress> lessonProgress = new Dictionary<int, LessonProgress>(); // Store progress here
    public List<GameObject> lessonItems = new List<GameObject>(); // List to store dynamically created lesson items
    public Dictionary<int, List<Word2>> levelWords = new Dictionary<int, List<Word2>>(); // Words assigned to each level
    public GameObject lessonsDetailsPage;
    public GameObject loading;
    private void Start()
    {
        levelWords=GameManager.Instance.levelWords;

        GenerateLessonUI(Constants.levelsCount);
    }
    private void OnEnable()
    {
        loading.SetActive(true);
        firebaseManager = FirebaseManager.Instance;
        firebaseManager.FetchAllLessonProgress(firebaseManager.user.UserId, OnProgressFetched);

        Invoke("UpdateLessonsProgress",.5f);
    }
   
    private void OnProgressFetched(Dictionary<int, LessonProgress> progress)
    {
        lessonProgress = progress;
        //Debug.Log("Fetched Progress: " + string.Join(", ", lessonProgress.Select(kvp => $"Level {kvp.Key}: {kvp.Value}")));
        
        
    }
    private void UpdateLessonsProgress()
    {
       
        //Debug.Log("Updating lesson progress...");
        for (int i = 1; i < Constants.levelsCount; i++)
        {
            if (lessonProgress.ContainsKey(i))
            {
                int zeroBasedIndex = i - 1;
                TextMeshProUGUI progressText = lessonItems[zeroBasedIndex].GetComponentsInChildren<TextMeshProUGUI>()[1];
                progressText.text = $"{lessonProgress[i].Progress}/{levelWords[i].Count}";
                //Debug.Log($"Level {i}: Updated Progress Text {lessonProgress[i]}/{levelWords[i].Count}");
                Button levelButton = lessonItems[zeroBasedIndex].GetComponentInChildren<Button>();
                levelButton.GetComponentInChildren<TextMeshProUGUI>().text = lessonProgress[i].Progress != levelWords[i].Count ? "Continue" : "Done";
                levelButton.GetComponent<Image>().color = lessonProgress[i].Progress != levelWords[i].Count ? Color.yellow : Color.green;

            }
            else
            {
                Button levelButton = lessonItems[i-1].GetComponentInChildren<Button>();
                levelButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
                //Debug.LogWarning($"No progress found for Level {i} in lessonProgress.");
            }
        }
        loading.SetActive(false);

    }


    private void GenerateLessonUI(int levelCount)
    {
        for (int i = 0; i < levelCount; i++)
        {
            GameObject lessonItem = Instantiate(lessonPrefab, lessonContentPanel);
            lessonItems.Add(lessonItem);
            LevelItemController levelItem= lessonItem.GetComponent<LevelItemController>();
           
            int level = i + 1;  // Levels start from 1
            levelItem.LevelText.text = $"Level {level}";

            // Log to ensure the level number and progress are being set correctly
            int progress = lessonProgress.ContainsKey(level) ? lessonProgress[level].Progress : 0;
            levelItem.progressText.text = $"{progress}/{levelWords[level].Count}";

            //Debug.Log($"Generated Level {level}: Initial Progress {progress}");
            

            levelItem.learnBTN.onClick.AddListener(() => OnLessonButtonClicked(level,progress));
        }

        // Log the number of lesson items created
        //Debug.Log($"Generated {lessonItems.Count} lesson items.");

    }




    public void OnLessonButtonClicked(int level,int progress)
    {
        Debug.Log(level+"_"+progress);
        // Handle lesson button click
        // Load the lesson for the clicked level, or show learning UI
        //Debug.Log($"Lesson {level} clicked.");
        lessonsDetailsPage.SetActive(true);
        lessonsDetailsPage.GetComponent<LevelWordsManager>().levelNumber = level;
        lessonsDetailsPage.GetComponent<LevelWordsManager>().currentWordIndex = progress;
        lessonsDetailsPage.GetComponent<LevelWordsManager>().levelWords = levelWords[level];
        lessonsDetailsPage.GetComponent<LevelWordsManager>().Setup();
        this.gameObject.SetActive(false);
    }


}
