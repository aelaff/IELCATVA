using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

public class CSVtoJSONConverter : MonoBehaviour
{
    

    void Start()
    {
        // Path to the CSV file (replace with your file path)
        string csvFilePath = "Assets/Scripts/dataset.csv";

        // Path to save the JSON file (replace with your desired path)
        string jsonFilePath = "Assets/Scripts/word_data.json";

        try
        {
            // Read all lines from the CSV file
            string[] csvLines = File.ReadAllLines(csvFilePath);

            // Skip the header line (assuming the first line is the header)
            string[] headers = csvLines[0].Split(',');

            // Create a list to store WordEntry objects
            List<Word2> wordEntries = new List<Word2>();

            // Process each line in the CSV file (skip the header)
            for (int i = 1; i < csvLines.Length; i++)
            {
                string[] values = csvLines[i].Split(',');

                // Create a WordEntry object from the CSV data
                Word2 wordEntry = new Word2
                {
                    Turkish = values[0],
                    EnglishMeaning = values[1].Trim('"'), // Remove double quotes
                    //Category = values[2].Trim(), // Trim whitespaces
                    //Difficulty = values[3]
                };

                // Add the WordEntry object to the list
                wordEntries.Add(wordEntry);
                Debug.Log(wordEntry.Turkish + " " + wordEntry.EnglishMeaning + " " + wordEntry.Category + " " + wordEntry.Difficulty);
            }
            /*
            // Group WordEntry objects by category
            var groupedByCategory = wordEntries.GroupBy(entry => entry.Category)
                                              .Select(group => new CategoryData
                                              {
                                                  Category = group.Key,
                                                  Words = group.OrderBy(word => word.Difficulty).ToList()
                                              })
                                              .OrderBy(category => category.Category.Trim()) // Trim whitespaces
                                              .ToList();

            // Serialize the grouped data to JSON
            string jsonData = JsonConvert.SerializeObject(groupedByCategory, Formatting.Indented);

            // Write the JSON data to a file
            File.WriteAllText(jsonFilePath, jsonData);

            Debug.Log("JSON file created successfully at: " + jsonFilePath);
            */
        }
        catch (IOException e)
        {
            Debug.LogError("Error reading/writing the file: " + e.Message);
        }
    }
}
