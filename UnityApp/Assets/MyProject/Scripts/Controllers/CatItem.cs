using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CatItem : MonoBehaviour
{
    public TextMeshProUGUI catName;
    public TextMeshProUGUI catPercentage;
    public Image catFillArea;
    private Button clickBtn;
    private int catId;
    private void Start()
    {
        clickBtn = GetComponent<Button>();
        clickBtn.onClick.AddListener(OpenLessonDetails);
    }
    public void Setup(string catName, float weaknessPercentage, int catID)
    {
        this.catName.text = catName;
        float roundedNumber = (float)Math.Round((weaknessPercentage * 100), 1, MidpointRounding.AwayFromZero);
        catPercentage.text = (100- roundedNumber) + "%";
        catFillArea.fillAmount = weaknessPercentage;
        catId = catID;
    }
    void OpenLessonDetails() {
        LevelWordsManager lessonsDetails=FindObjectsOfType<LevelWordsManager>(true).FirstOrDefault();
        HomeController homePage = FindAnyObjectByType<HomeController>();
        lessonsDetails.gameObject.SetActive(true);
        lessonsDetails.levelNumber = 1000;
        lessonsDetails.currentWordIndex = 0;
        List<Word2> filteredWords = GameManager.Instance.allWords.Where(word => word.Category == catId).ToList();

        lessonsDetails.levelWords = filteredWords;
        lessonsDetails.Setup();
        homePage.gameObject.SetActive(false);
    }


}
