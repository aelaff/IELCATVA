using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    int rank = 1;
    private void Start()
    {
        LoadLeaderboard();
    }

    public void LoadLeaderboard()
    {
        FirebaseManager.Instance.FetchLeaderboard(leaderboardData =>
        {
            if (leaderboardData == null)
            {
                Debug.LogError("Failed to load leaderboard data.");
                return;
            }

            foreach (Transform child in leaderboardContent)
            {
                Destroy(child.gameObject);
            }

            foreach (var userProfile in leaderboardData)
            {
                GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                entry.GetComponent<LeaderboardEntry>().Setup(userProfile);
                entry.GetComponent<LeaderboardEntry>().SetRank(rank++);

            }
        });
    }
}
