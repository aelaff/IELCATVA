using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SyllableCounter : MonoBehaviour
{
    int CountSyllables(string word)
    {
        char[] vowels = { 'a', 'e', 'ı', 'i', 'o', 'ö', 'u', 'ü' };

        word = word.ToLower();

        int syllableCount = 0;
        bool prevCharWasVowel = false;

        foreach (char c in word)
        {
            if (System.Array.Exists(vowels, v => v == c))
            {
                if (!prevCharWasVowel)
                {
                    syllableCount++;
                }
                prevCharWasVowel = true;
            }
            else
            {
                prevCharWasVowel = false;
            }
        }

        if (word.EndsWith("n") || word.EndsWith("m"))
        {
            syllableCount++;
        }

        syllableCount = Mathf.Max(syllableCount, 1);

        return syllableCount;
    }

    int DetermineDifficulty(string word)
    {
        int syllableCount = CountSyllables(word);

        if (syllableCount <= 2)
        {
            return 1;  // Easy
        }
        else if (syllableCount <= 3)
        {
            return 2;  // Moderate
        }
        else if (syllableCount <= 4)
        {
            return 3;  // Challenging
        }
        else if (syllableCount <= 5)
        {
            return 4;  // Difficult
        }
        else
        {
            return 5;  // Very Difficult
        }
    }

    void Start()
    {
        string inputFilePath = "Assets/Scripts/WordList.txt";
        string outputFilePath = "Assets/Scripts/DifficultyOutput.txt";

        try
        {
            string[] words = File.ReadAllLines(inputFilePath);

            var difficultyLines = new List<string>();

            foreach (string word in words)
            {
                int difficulty = CountSyllables(word);
                difficultyLines.Add(difficulty+"");
            }

            File.WriteAllLines(outputFilePath, difficultyLines.ToArray());
            Debug.Log("Difficulty information written to: " + outputFilePath);
        }
        catch (IOException e)
        {
            Debug.LogError("Error reading/writing the file: " + e.Message);
        }
    }
}
