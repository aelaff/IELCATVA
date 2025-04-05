using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using TMPro;

public class CategoryManager : MonoBehaviour
{
    public GameObject categoryPrefab;
    public Transform contentParent;
    public List<Category> selectedCategories = new List<Category>();
    List<CategoryItemUI> allItemsCategories;
    public TextMeshProUGUI buttonText;
    bool isAllSelected = false;
    void Start()
    {
        allItemsCategories = new List<CategoryItemUI>();

        // Replace with the path to your categories CSV file
        string csvFilePath = "Assets/Resources/new/categories.csv";

        // Read CSV file and parse categories
        using (var reader = new StreamReader(csvFilePath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                // Assuming the CSV file has columns: id,name,image_path
                if (values.Length >= 3)
                {
                    int id;
                    if (int.TryParse(values[0], out id))
                    {
                        Category category = new Category
                        {
                            id = id,
                            name = values[1],
                            image = values[2]
                        };

                        // Instantiate category item
                        GameObject categoryItem = Instantiate(categoryPrefab, contentParent);
                        CategoryItemUI categoryItemUI = categoryItem.GetComponent<CategoryItemUI>();
                        categoryItemUI.SetCategoryInfo(category);
                        categoryItemUI.OnCategorySelected += ToggleCategorySelection;
                        allItemsCategories.Add(categoryItemUI);
                    }
                }
            }
        }
    }

    public void ChooseAllCategories()
    {
        isAllSelected = !isAllSelected;
        if (isAllSelected)
            buttonText.text = "Clear All";
        else
            buttonText.text = "Choose All";

        foreach (CategoryItemUI categoryItemUI in allItemsCategories) {
            categoryItemUI.OnItemClick();
        }

    }
    void ToggleCategorySelection(Category category)
    {
        if (selectedCategories.Contains(category))
        {
            selectedCategories.Remove(category);
        }
        else
        {
            selectedCategories.Add(category);
        }
    }
}
