using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI scoreText;

    public void Setup(UserProfile userProfile)
    {
        usernameText.text = userProfile.name;
        scoreText.text = GameManager.Instance.GetFormattedScore(userProfile.Score);
    }

    public void SetRank(int rank)
    {
        rankText.text = rank.ToString();
    }
}
