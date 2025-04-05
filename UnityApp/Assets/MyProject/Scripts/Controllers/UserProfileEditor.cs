using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UserProfileEditor : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField nameInput;
    //public TMP_InputField photoInput;
    public Toggle maleToggle; // Radio button for Male
    public Toggle femaleToggle; // Radio button for Female
    public TMP_InputField birthdateInput;
    public TMP_InputField addressInput;
    public TMP_InputField phoneNumberInput;
    //public TMP_InputField scoreInput;
    public Button saveButton, logOut;

    private FirebaseManager firebaseManager;

    private void Start()
    {
        firebaseManager = FirebaseManager.Instance;

        if (firebaseManager != null && firebaseManager.currentUserProfile != null)
        {
            PopulateInputFields(firebaseManager.currentUserProfile);
        }

        saveButton.onClick.AddListener(UpdateUserProfile);
        logOut.onClick.RemoveAllListeners();
        logOut.onClick.AddListener(SignOut);
    }
    void SignOut()
    {
        firebaseManager.SignOut();
        SceneManager.LoadScene("APPLogin");
    }

    private void PopulateInputFields(UserProfile userProfile)
    {
        emailInput.text = userProfile.email;
        nameInput.text = userProfile.name;
        //photoInput.text = userProfile.photo;

        // Set the radio button for gender based on the current profile
        if (userProfile.gender == 0) // Male
        {
            maleToggle.isOn = true;
        }
        else if (userProfile.gender == 1) // Female
        {
            femaleToggle.isOn = true;
        }

        birthdateInput.text = userProfile.birthdate;
        addressInput.text = userProfile.address;
        phoneNumberInput.text = userProfile.phoneNumber;
        //scoreInput.text = userProfile.score.ToString();
    }

    private void UpdateUserProfile()
    {
        string email = emailInput.text;
        string name = nameInput.text;
        //string photo = photoInput.text;

        // Get the selected gender from radio buttons
        int gender = maleToggle.isOn ? 0 : (femaleToggle.isOn ? 1 : -1); // Default to -1 if none selected, handle errors if needed

        string birthdate = birthdateInput.text;
        string address = addressInput.text;
        string phoneNumber = phoneNumberInput.text;
        //int score = int.Parse(scoreInput.text); // Assuming input is valid

        // Create a new user profile object with the updated data
        UserProfile updatedProfile = new UserProfile(
            firebaseManager.user.UserId, email, name, "photo", gender, birthdate, address, phoneNumber, GameManager.Instance.currentScore
        );

        // Save the updated profile to Firebase
        firebaseManager.SaveUserProfile(updatedProfile, (success) =>
        {
            if (success)
            {
                Debug.Log("User profile updated successfully.");
            }
            else
            {
                Debug.LogError("Failed to update user profile.");
            }
        });
    }
}
