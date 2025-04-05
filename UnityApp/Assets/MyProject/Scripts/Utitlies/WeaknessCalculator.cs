using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class WeaknessCalculator : MonoBehaviour
{
    private string inputCsvFilePath = "Assets/Resources/new/withoutWeakness.csv";
    private string outputCsvFilePath = "Assets/Resources/new/withWeakness.csv";
    public APIManager apiManager;
    //private List<DataEntry> dataEntries = new List<DataEntry>();

    public void Start()
    {
        apiManager=GetComponent<APIManager>();
        //LoadCSV();
        //CalculateWeakness(dataEntries);
        //CalculateCategoryWeakness();

    }

    //private void LoadCSV()
    //{
    //    if (!File.Exists(inputCsvFilePath))
    //    {
    //        Debug.LogError("CSV file not found at " + inputCsvFilePath);
    //        return;
    //    }

    //    string[] lines = File.ReadAllLines(inputCsvFilePath);
    //    for (int i = 1; i < lines.Length; i++)
    //    {
    //        Debug.Log(lines[i]);
    //        string[] parts = lines[i].Split(',');
    //        DataEntry entry = new DataEntry
    //        {
    //            WordID = int.Parse(parts[0]),
    //            CategoryID = int.Parse(parts[1]),
    //            //TurkishWord = parts[2],
    //            CorrectAnswer = int.Parse(parts[3]),
    //            TimeSpent = float.Parse(parts[4], CultureInfo.InvariantCulture),
    //            HesitationTimes = int.Parse(parts[5]),
    //            Difficulty = int.Parse(parts[6]),
    //            UserId = parts[7]
    //        };
    //        dataEntries.Add(entry);
    //    }
    //    Debug.Log("CSV loaded successfully!");
    //}

    public void CalculateWeakness(List<DataEntry> dataEntries)
    {
        float maxTimeSpent = dataEntries.Max(entry => entry.TimeSpent);
        int maxHesitationTimes = dataEntries.Max(entry => entry.HesitationTimes);
        float maxDifficulty = dataEntries.Max(entry => entry.Difficulty);

        foreach (var entry in dataEntries)
        {
            float weaknessScore = CalculateWeaknessScore(
                entry.CorrectAnswer == 1,
                entry.TimeSpent,
                maxTimeSpent,
                entry.HesitationTimes,
                maxHesitationTimes,
                entry.Difficulty,
                maxDifficulty,
                0.5f);

            entry.Weakness = weaknessScore > 0.5f ? 1 : 0;
            
        }
        //apiManager.TrainData(dataEntries);
        //SaveCSV(dataEntries);

    }
    public List<CategoryWeakness> GetCategoryWeakness(List<DataEntry> dataEntries)
    {
        // Grouping DataEntry by CategoryID
        var groupedByCategory = dataEntries
            .GroupBy(entry => entry.CategoryID)
            .Select(group => new
            {
                CategoryID = group.Key,
                MeanWeakness = (float)group.Average(entry => entry.Weakness)
            })
            .Where(group => group.MeanWeakness < 0.5f)  // Filter those with mean weakness less than 0.5
            .ToList();

        // Convert to CategoryWeakness list
        List<CategoryWeakness> categoryWeaknessList = groupedByCategory
            .Select(group => new CategoryWeakness
            {
                CategoryID = group.CategoryID,
                MeanWeakness = group.MeanWeakness
            })
            .ToList();

        return categoryWeaknessList;
    }


    public static float CalculateWeaknessScore(
        bool correctAnswer,
        float timeSpent,
        float maxTimeSpent,
        int hesitationTimes,
        int maxHesitationTimes,
        float difficulty,
        float maxDifficulty,
        float difficultyThreshold = 0.5f)
    {
        float wCorrect = difficulty >= difficultyThreshold ? 0.6f : 0.4f;
        float wTime = difficulty >= difficultyThreshold ? 0.1f : 0.3f;
        float wHesitation = 0.2f;
        float wDifficulty = 0.1f;

        float normalizedTime = maxTimeSpent > 0 ? timeSpent / maxTimeSpent : 0f;
        float normalizedHesitation = maxHesitationTimes > 0 ? (float)hesitationTimes / maxHesitationTimes : 0f;
        float normalizedDifficulty = maxDifficulty > 0 ? difficulty / maxDifficulty : 0f;

        float weaknessScore =
            wCorrect * (correctAnswer ? 0f : 1f) +
            wTime * normalizedTime +
            wHesitation * normalizedHesitation +
            wDifficulty * normalizedDifficulty;

        return weaknessScore;
    }

    private void SaveCSV(List<DataEntry> dataEntries)
    {
        FileMode fileMode = File.Exists(outputCsvFilePath) ? FileMode.Append : FileMode.CreateNew;

        using (StreamWriter writer = new StreamWriter(outputCsvFilePath, true))
        {
            if (fileMode == FileMode.CreateNew)
            {
                writer.WriteLine("WordID,CategoryID,CorrectAnswer,TimeSpent,HesitationTimes,Difficulty,UserId,Weakness");
            }

            foreach (var entry in dataEntries)
            {

                writer.WriteLine(entry.ToCSVString());
            }
        }


        Debug.Log("CSV file updated successfully!");
    }
}
[Serializable]
public class CategoryWeakness
{
    public int CategoryID;
    public float MeanWeakness;

}
[Serializable]
public class DataEntry
{
    public int WordID;
    public int CategoryID;
    public string TurkishWord;
    public int CorrectAnswer;
    public float TimeSpent;
    public int HesitationTimes;
    public int Difficulty;
    public string UserId;
    public int Weakness;
    public string ToCSVString()
    {
        return string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
            WordID, CategoryID, CorrectAnswer, TimeSpent, HesitationTimes, Difficulty, UserId, Weakness);
    }
}
