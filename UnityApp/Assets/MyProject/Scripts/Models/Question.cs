
using System.Collections.Generic;

[System.Serializable]
public class Question
{
    public int WordId;
    public int CategoryId;
    public string TurkishWord;
    public List<string> Choices;
    public string CorrectChoice;
    public bool IsCorrect;
    public float TimeSpent;
    public int NumberOfClickedAnswers;
    public int Difficulty;

    public string ToCSVString()
    {
        return string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
            WordId, CategoryId, TurkishWord, IsCorrect ? 1 : 0, TimeSpent, NumberOfClickedAnswers, Difficulty,FirebaseManager.Instance.user.UserId);
    }
}