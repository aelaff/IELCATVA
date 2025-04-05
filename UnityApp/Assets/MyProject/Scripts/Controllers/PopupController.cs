using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    public TextMeshProUGUI titleText; 
    public TextMeshProUGUI descriptionText; 
    public Image signImage; 

    public Sprite rightSprite; 
    public Sprite wrongSprite;
    public Button actionBtn;

    public void ShowPopup(string title, string description, bool isRight)
    {
        titleText.text = title;
        descriptionText.text = description;

        if (isRight)
        {
            signImage.sprite = rightSprite;
        }
        else
        {
            signImage.sprite = wrongSprite;
        }

        gameObject.SetActive(true);
    }

    public void HidePopup()
    {
        gameObject.SetActive(false);
    }
}
