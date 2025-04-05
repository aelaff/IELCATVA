using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UserProfileDisplay : MonoBehaviour
{
    public TextMeshProUGUI emailText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI photoText;
    public TextMeshProUGUI genderText;
    public TextMeshProUGUI birthdateText;
    public TextMeshProUGUI addressText;
    public TextMeshProUGUI phoneNumberText;
    public TextMeshProUGUI scoreText;
    public Button logOut;
    private FirebaseManager firebaseManager;

    private void Start()
    {
        firebaseManager = FirebaseManager.Instance;

        if (firebaseManager != null)
        {
            // Display user profile data
            DisplayUserProfile(firebaseManager.currentUserProfile);
        }
        logOut.onClick.RemoveAllListeners();
        logOut.onClick.AddListener(SignOut);
    }
    void SignOut()
    {
        firebaseManager.SignOut();
        SceneManager.LoadScene("APPLogin");
    }
    private void OnEnable()
    {
        firebaseManager = FirebaseManager.Instance;

        if (firebaseManager != null)
        {
            // Display user profile data
            DisplayUserProfile(firebaseManager.currentUserProfile);
        }
    }
    private void DisplayUserProfile(UserProfile userProfile)
    {
        if (userProfile != null)
        {
            emailText.text = userProfile.email;
            nameText.text = userProfile.name;
            //photoText.text = "Photo: " + userProfile.photo;
            genderText.text =  (userProfile.gender == 0 ? "Male" : "Female");
            birthdateText.text =  userProfile.birthdate;
            addressText.text =  userProfile.address;
            phoneNumberText.text =  userProfile.phoneNumber;
            scoreText.text = "" + userProfile.score;
        }
    }
}
