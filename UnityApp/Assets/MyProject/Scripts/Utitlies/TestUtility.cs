using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class TestUtility : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MapCategories("Assets/MyProject/Resources/new/main_dataset.csv", "Assets/MyProject/Resources/new/main_dataset2.csv", "Assets/MyProject/Resources/new/categories.csv");
        //ConvertCsvToJson("Assets/MyProject/Resources/new/categorized_main_dataset.csv", "Assets/MyProject/Resources/new/categorized_main_dataset.json", "Assets/MyProject/Resources/new/categorized_main_dataset.csv");

    }
    public static void MapCategories(string inputFilePath, string outputFilePath, string categoryMappingFilePath)
    {
        Dictionary<string, int> categoryIds = new Dictionary<string, int>();
        List<string[]> newRows = new List<string[]>();
        string[] header = null;

        using (var reader = new StreamReader(inputFilePath))
        {
            int categoryId = 1; // Start category IDs from 1

            // Read the header
            header = reader.ReadLine().Split(';');

            // Find the column index containing category names
            int categoryIndex = Array.IndexOf(header, "Category");

            // Check if the "Category" column exists in the CSV file
            if (categoryIndex == -1)
            {
                Console.WriteLine("Category column not found in the CSV file.");
                return;
            }

            // Process each row
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                // Check if the values array has enough elements
                if (values.Length > categoryIndex)
                {
                    string category = values[categoryIndex];

                    // Assign ID to category if not already assigned
                    if (!categoryIds.ContainsKey(category))
                    {
                        categoryIds[category] = categoryId++;
                    }

                    // Replace category name with ID
                    values[categoryIndex] = categoryIds[category].ToString();

                    newRows.Add(values);
                }
                else
                {
                    Console.WriteLine("Row does not contain enough columns.");
                }
            }
        }

        // Write the updated CSV file
        using (var writer = new StreamWriter(outputFilePath))
        {
            // Write header
            writer.WriteLine(string.Join(";", header));

            // Write rows with replaced category names
            foreach (var row in newRows)
            {
                writer.WriteLine(string.Join(";", row));
            }
        }

        // Write the category ID and name mapping CSV file
        using (var writer = new StreamWriter(categoryMappingFilePath))
        {
            writer.WriteLine("CategoryId;CategoryName;CategoryImage");

            foreach (var kvp in categoryIds)
            {
                writer.WriteLine($"{kvp.Value};{kvp.Key};0");
            }
        }
    }
    public static void ConvertCsvToJson(string csvFilePath, string jsonFilePath, string categoryMappingFilePath)
    {
        // Read the CSV file into memory
        List<string[]> rows = new List<string[]>();
        using (var reader = new StreamReader(csvFilePath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');
                rows.Add(values);
            }
        }

        // Read the category mapping file into a dictionary
        Dictionary<int, string> categoryMapping = new Dictionary<int, string>();
        using (var reader = new StreamReader(categoryMappingFilePath))
        {
            reader.ReadLine(); // Skip the header
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');
                categoryMapping[int.Parse(values[0])] = values[1];
            }
        }

        // Create a dictionary to hold the word information
        Dictionary<string, List<Word>> categories = new Dictionary<string, List<Word>>();
        foreach (var row in rows)
        {
            int categoryId;
            if (int.TryParse(row[1], out categoryId))
            {
                string category = categoryMapping[categoryId];

                if (!categories.ContainsKey(category))
                {
                    categories[category] = new List<Word>();
                }

                categories[category].Add(new Word
                {
                    WordId = int.Parse(row[0]),
                    CategoryId = categoryId,
                    Turkish = row[2],
                    EnglishMeaning = row[3],
                    Difficulty = int.Parse(row[4])
                });
            }
            else
            {
                // Handle the case where row[1] is not a valid integer
                Console.WriteLine($"Invalid category ID at row {string.Join(";", row)}");
            }
        }

        // Serialize the categories dictionary to JSON
        string json = JsonConvert.SerializeObject(categories, Formatting.Indented);

        // Write the JSON to a file
        File.WriteAllText(jsonFilePath, json);
    }
}
