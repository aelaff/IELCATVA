using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static FirebaseManager;

public class HomeController : MonoBehaviour
{
    FirebaseManager firebaseManager;
    public Dictionary<int, LessonProgress> lessonProgress = new Dictionary<int, LessonProgress>(); // Store progress here
    public Dictionary<int, int> excersizeProgress = new Dictionary<int, int>(); // Store progress here

    int finishedLevels, finishedWords, levelInProgress, exersicesInProgress, finishedExersices;
    public GameObject loading;
    public Slider allLevelsProgress, unFinishedLevels, unFinishedExercizes;
    public TextMeshProUGUI finshedWordsTxt, finishedLevelsTxt, levelsInProgressTxt, exercizesInProgressTxt,
        scoreTxt;
    public Transform weaknessSection, weaknessItem;
    public GameObject noProgressCategories, progressCategories;
    void OnEnable()
    {
        finishedLevels = 0;
        finishedWords = 0;
        levelInProgress = 0;
        exersicesInProgress = 0;
        finishedExersices = 0;
        loading.SetActive(true);
        firebaseManager = FirebaseManager.Instance;
        firebaseManager.FetchAllLessonProgress(firebaseManager.user.UserId, OnProgressFetched);
        firebaseManager.FetchAllExercizesMarks(firebaseManager.user.UserId, OnExcersizesFetched);
        firebaseManager.LoadCategoryWeakness(firebaseManager.user.UserId, OnWeaknessCategoriesFetched);
        Invoke("UpdateLessonsProgress", .5f);

    }
    void UpdateLessonsProgress() {
        allLevelsProgress.value = float.Parse(finishedLevels + "") / float.Parse(Constants.levelsCount + "");
        finshedWordsTxt.text = $"Learned Words: {finishedWords}";
        finishedLevelsTxt.text = $"Completed Levels: {finishedLevels}";
        unFinishedLevels.value = float.Parse(levelInProgress + "") / float.Parse(Constants.levelsCount + "");
        levelsInProgressTxt.text = $"{levelInProgress}/{Constants.levelsCount}";

        unFinishedExercizes.value = float.Parse(exersicesInProgress + "") / float.Parse(Constants.levelsCount + "");
        exercizesInProgressTxt.text = $"{exersicesInProgress}/{Constants.levelsCount}";

        scoreTxt.text = GameManager.Instance.GetFormattedScore(GameManager.Instance.currentScore);
        loading.SetActive(false);

    }
    private void OnProgressFetched(Dictionary<int, LessonProgress> progress)
    {
        //lessonProgress = progress;
        //Debug.Log("Fetched Progress: " + string.Join(", ", lessonProgress.Select(kvp => $"Level {kvp.Key}: {kvp.Value}")));
        //GetAllFinishedWords();
        foreach (var item in progress)
        {
            //int levelsWordCount = GameManager.Instance.levelWords[item.Key].Count;
            //Debug.Log(item.Key+"_"+levelsWordCount);
            finishedWords += item.Value.Progress;
            if (item.Value.Progress == item.Value.TotalLessons)
                finishedLevels++;
            else
                levelInProgress++;
        }


    }
    private void OnWeaknessCategoriesFetched(List<CategoryWeakness> categories) {
        if (categories.Count == 0)
        {
            noProgressCategories.SetActive(true);
            progressCategories.SetActive(false);
        }
        else {
            noProgressCategories.SetActive(false);
            progressCategories.SetActive(true);
        }
        foreach (CategoryWeakness category in categories) {
            var matchingCategory = GameManager.Instance.categoriesList.FirstOrDefault(cat => cat.id == category.CategoryID);
            string matchCat = matchingCategory.name.Replace("_", " & ");
            GameObject cat =Instantiate(weaknessItem, weaknessSection).gameObject;
            cat.GetComponent<CatItem>().Setup(matchCat, category.MeanWeakness,category.CategoryID);
        }
    }
    private void OnExcersizesFetched(Dictionary<int, int> progress)
    {
        excersizeProgress = progress;
        GetAllExercises();

    }
    private void GetAllExercises()
    {
        foreach (var item in excersizeProgress)
        {
            int levelsWordCount = GameManager.Instance.levelWords[item.Key].Count;
            if (item.Value == levelsWordCount)
                finishedExersices++;
            else
                exersicesInProgress++;
        }
    }
    private void GetAllFinishedWords() {
        foreach (var item in lessonProgress)
        {
            int levelsWordCount = GameManager.Instance.levelWords[item.Key].Count;
            //Debug.Log(item.Key+"_"+levelsWordCount);
            finishedWords += item.Value.Progress;
            if (item.Value.Progress == levelsWordCount)
                finishedLevels++;
            else
                levelInProgress++;
        }
    }
   
   
    
}
