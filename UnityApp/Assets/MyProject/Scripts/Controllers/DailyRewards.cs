using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewards : MonoBehaviour
{
    [Serializable]
    public class DayReward
    {
        public string dayName;
        public int gemAmount;
        public Button claimButton;
        public GameObject lockOverlay;
        public TextMeshProUGUI gemText;
    }

    public DayReward[] dailyRewards; // 7 rewards for the week
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionText;
    public GameObject rewardsScreen;

    private int currentDayIndex;
    private const string LastClaimDateKey = "LastClaimDate";
    private const string CurrentDayKey = "CurrentDayIndex";

    void Start()
    {
        if (FirebaseManager.Instance.isGuest)
            return;
        PlayerPrefs.DeleteAll();
        LoadDailyRewardData();

        if (ShouldShowRewardsScreen())
        {
            rewardsScreen.SetActive(true);
            InitializeUI();
        }
        else
        {
            rewardsScreen.SetActive(false);
        }
    }

    void InitializeUI()
    {
        for (int i = 0; i < dailyRewards.Length; i++)
        {
            int index = i; // Prevent closure issue in lambdas
            dailyRewards[i].gemText.text = dailyRewards[i].gemAmount.ToString();
            dailyRewards[i].lockOverlay.SetActive(i != currentDayIndex); // Lock all except today's reward
            dailyRewards[i].claimButton.interactable = i == currentDayIndex;
            dailyRewards[i].claimButton.onClick.RemoveAllListeners();
            dailyRewards[i].claimButton.onClick.AddListener(() => ClaimReward(index));
        }
    }

    void ClaimReward(int dayIndex)
    {
        if (dayIndex != currentDayIndex) return;

        int claimedGems = dailyRewards[dayIndex].gemAmount;
        Debug.Log($"Claimed {claimedGems} gems!");

        // Add claimed gems to the GameManager
        GameManager.Instance.AddGemsToScore(claimedGems);

        rewardsScreen.SetActive(false);


        // Save claim date and move to the next day
        PlayerPrefs.SetString(LastClaimDateKey, DateTime.Now.ToString());
        PlayerPrefs.SetInt(CurrentDayKey, ++currentDayIndex);

        // If the week ends, reset to the first day
        if (currentDayIndex >= dailyRewards.Length)
        {
            currentDayIndex = 0;
            PlayerPrefs.SetInt(CurrentDayKey, 0);
        }

        InitializeUI();
    }


    void LoadDailyRewardData()
    {
        string lastClaimDate = PlayerPrefs.GetString(LastClaimDateKey, "");
        DateTime lastClaimDateTime;

        if (DateTime.TryParse(lastClaimDate, out lastClaimDateTime))
        {
            if (lastClaimDateTime.Date < DateTime.Now.Date)
            {
                currentDayIndex = PlayerPrefs.GetInt(CurrentDayKey, 0);
            }
        }
        else
        {
            // First-time setup
            currentDayIndex = 0;
            PlayerPrefs.SetInt(CurrentDayKey, 0);
        }
    }

    bool ShouldShowRewardsScreen()
    {
        string lastClaimDate = PlayerPrefs.GetString(LastClaimDateKey, "");
        DateTime lastClaimDateTime;

        if (DateTime.TryParse(lastClaimDate, out lastClaimDateTime))
        {
            // Show screen if a new day has started
            return lastClaimDateTime.Date < DateTime.Now.Date;
        }

        return true; // Show the screen for the first time
    }
}
