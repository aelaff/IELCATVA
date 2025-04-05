using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BottomNavigationBar : MonoBehaviour
{
    [SerializeField] private Button[] navigationButtons;   // Array of buttons
    [SerializeField] private GameObject[] pages;           // Array of pages (e.g., Home, Leaderboard)
    [SerializeField] private Color selectedColor = new Color(0.5f, 0, 1); // Purple color
    [SerializeField] private Color deselectedColor = Color.black; // Black color

    private int currentPageIndex = 0;

    private void Start()
    {
        for (int i = 0; i < navigationButtons.Length; i++)
        {
            int index = i;
            navigationButtons[i].onClick.AddListener(() => SwitchPage(index));
        }

        SwitchPage(0); // Open the first page by default
    }

    private void SwitchPage(int pageIndex)
    {
        // If the page is already the current one, no need to switch
        if (currentPageIndex == pageIndex) return;

        // Disable all pages
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }

        // Enable the selected page
        pages[pageIndex].SetActive(true);

        // Update the current page index
        currentPageIndex = pageIndex;

        // Update button visuals (color of the icons and text)
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        for (int i = 0; i < navigationButtons.Length; i++)
        {
            var buttonImage = navigationButtons[i].transform.GetChild(1).GetComponent<Image>();
            var buttonText = navigationButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            // Update the button icon and text color
            if (i == currentPageIndex)
            {
                buttonImage.color = selectedColor; // Set icon to purple
                buttonText.color = selectedColor;  // Set text to purple
            }
            else
            {
                buttonImage.color = deselectedColor; // Set icon to black
                buttonText.color = deselectedColor;  // Set text to black
            }
        }
    }
}
