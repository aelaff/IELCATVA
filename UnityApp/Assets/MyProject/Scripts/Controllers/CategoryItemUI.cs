using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CategoryItemUI : MonoBehaviour
{
    public TextMeshProUGUI categoryNameText;
    public Image categoryImage;
    public Button categoryBtn;
    private bool isSelected = false;

    private void Awake()
    {
        categoryBtn = GetComponent<Button>();
        categoryBtn.onClick.AddListener(OnItemClick);
    }

    public delegate void CategorySelectedHandler(Category category);
    public event CategorySelectedHandler OnCategorySelected;

    private Category currentCategory;

    public void SetCategoryInfo(Category category)
    {
        currentCategory = category;
        categoryNameText.text = category.name;

        // Load and set category image here (you may use a library like Unity's Addressables or Resources.Load)
        // categoryImage.sprite = LoadCategoryImage(category.image);

        // You can also add additional UI customization based on your prefab structure
    }

    public void OnItemClick()
    {

        isSelected = !isSelected;
        OnCategorySelected?.Invoke(currentCategory);
        UpdateSelectionState();

    }
    private void UpdateSelectionState()
    {
        Color highlightColor = new Color(132f / 255f, 32f / 255f, 253f / 255f); // #8420FD in RGB

        if (isSelected)
        {
            categoryBtn.image.color = highlightColor;
            categoryNameText.color = Color.white;

        }
        else
        {
            categoryBtn.image.color = Color.white;
            categoryNameText.color = Color.gray;

        }
    }
}
