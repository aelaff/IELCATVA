using System.Collections.Generic;

[System.Serializable]
public class UserProfile
{
    public string userId;
    public string email;
    public string name;
    public string photo;
    public int gender;
    public string birthdate;
    public string address;
    public string phoneNumber;
    public int score;
    public Dictionary<int, int> lessonProgress = new Dictionary<int, int>();
    public int Score
    {
        get { return score; }
        set
        {
            score = value;
            FirebaseManager.Instance.SaveUserProfile(FirebaseManager.Instance.currentUserProfile, null);

        }
    }
    public UserProfile(string userId, string email, string name, string photo, int gender, string birthdate, string address, string phoneNumber, int score)
    {
        this.userId = userId;
        this.email = email;
        this.name = name;
        this.photo = photo;
        this.gender = gender;
        this.birthdate = birthdate;
        this.address = address;
        this.phoneNumber = phoneNumber;
        this.score = score;
    }
    
}


    
