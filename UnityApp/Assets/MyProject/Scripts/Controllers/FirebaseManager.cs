using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Storage;
using Google;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public string mainScene = "Main";
    public string loginScene = "Login";
    private FirebaseAuth auth;
    public FirebaseUser user;
    public string storageBucketURL = "gs://learnturkish-3b2a2.appspot.com";
    DatabaseReference databaseReference;
    //private StorageReference storageReference;
    //private FirebaseStorage storage;
    public delegate void DisplayErrorMessageDelegate(string title, string message, bool isSuccess);
    public static event DisplayErrorMessageDelegate OnDisplayErrorMessage;
    private GameObject loading;

    public UserProfile currentUserProfile;
    public bool isGuest = false;
    public enum ServiceProvider
    {
        Guest,
        UsernamePassword,
        Apple,
        Google

    }
    public ServiceProvider service;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            //storage = FirebaseStorage.GetInstance(app);

            //storageReference = storage.GetReferenceFromUrl(storageBucketURL);

        });
        loading = GameObject.Find("Loading").transform.GetChild(0).gameObject;
        InitializeFirebase();
    }
    private void Start()
    {

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
        loading = GameObject.Find("Loading").transform.GetChild(0).gameObject;
    }
    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }
    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        loading.SetActive(true);
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                StartCoroutine(LoadSceneAfterDelay(loginScene));
                this.currentUserProfile = null;
                Debug.Log("Signed out " + user.UserId);

            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                Instance.FetchUserProfile(user.UserId, HandleUserProfile);
            }
        }
    }
    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(0f);
        loading.SetActive(false);
        SceneManager.LoadScene(sceneName);
    }
    private void HandleUserProfile(UserProfile userProfile)
    {
        //loading = GameObject.Find("Loading").transform.GetChild(0).gameObject;
        if (!isGuest)
        {
            if (userProfile != null)
            {
                this.currentUserProfile = userProfile;
            }
            else
            {
                this.currentUserProfile = new UserProfile(user.UserId, user.Email, user.DisplayName, "", 0, "", "", "", currentUserProfile.Score);
                SaveUserProfile(currentUserProfile, null);
                Debug.LogWarning("No user data found for the current user ID.");
            }
        }
        else
        {
            this.currentUserProfile = new UserProfile(user.UserId, "guest@gazi.edu.tr", "guest", "", 0, "", "", "", 0);

        }
        StartCoroutine(LoadSceneAfterDelay(mainScene));

    }
    public void RegisterUser(string username, string password)
    {
        isGuest = false;

        loading.SetActive(true);

        auth.CreateUserWithEmailAndPasswordAsync(username, password).ContinueWithOnMainThread(task =>
        {
            loading.SetActive(false);

            if (task.IsCanceled || task.IsFaulted)
            {
                OnDisplayErrorMessage?.Invoke("Failed to register user!", GetErrorMessage(task.Exception), false);
                return;
            }
            service = ServiceProvider.UsernamePassword;

            AuthResult result = task.Result;
            if (result != null)
            {
                SetUserDetails(result.User);
                StartCoroutine(LoadSceneAfterDelay(mainScene));

            }
        });
    }

    public void Login(string username, string password)
    {
        loading.SetActive(true);
        isGuest = false;
        service = ServiceProvider.UsernamePassword;

        auth.SignInWithEmailAndPasswordAsync(username, password).ContinueWithOnMainThread(task =>
        {
            loading.SetActive(false);

            if (task.IsCanceled || task.IsFaulted)
            {
                OnDisplayErrorMessage?.Invoke("Failed to login!", GetErrorMessage(task.Exception), false);
                return;
            }

            AuthResult result = task.Result;
            SceneManager.LoadScene("APPMain");

        });
    }
    public void SignInAnonymously()
    {
        isGuest = true;
        loading.SetActive(true);
        service = ServiceProvider.Guest;

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            loading.SetActive(false);

            if (task.IsCanceled || task.IsFaulted)
            {
                OnDisplayErrorMessage?.Invoke("Failed to login!", GetErrorMessage(task.Exception), false);
                return;
            }

            AuthResult result = task.Result;
            if (result != null)
            {
                SetUserDetails(result.User);
                StartCoroutine(LoadSceneAfterDelay(mainScene));
            }
        });
    }
    public void FirebaseGoogleLogin(string googleIdToken, string googleAccessToken)
    {
        loading.SetActive(true);
        isGuest = false;
        service = ServiceProvider.Google;

        Credential credential = GoogleAuthProvider.GetCredential(googleIdToken, googleAccessToken);
        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
            loading.SetActive(false);

            if (task.IsCanceled)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            SetUserDetails(result.User);
            Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
        });
    }
    public void FirebaseAppleLogin(string appleIdToken, string rawNonce)
    {
        loading.SetActive(true);
        isGuest = false;
        service = ServiceProvider.Apple;

        Credential credential = OAuthProvider.GetCredential("apple.com", appleIdToken, rawNonce, null);
        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
            loading.SetActive(false);

            if (task.IsCanceled)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            SetUserDetails(result.User);
            Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
        });
    }
    public void SetUserDetails(FirebaseUser currentUser)
    {
        currentUserProfile = new UserProfile(currentUser.UserId, currentUser.Email, currentUser.DisplayName, "", 0, "", "", "", currentUserProfile.Score);

    }
    public void SignOut()
    {
        if (service == ServiceProvider.Google)
        {
            //GoogleSignIn.DefaultInstance.SignOut();
        }

        auth.SignOut();

    }
    public void SaveUserProfile(UserProfile userProfile, Action<bool> callback)
    {
        string json = JsonUtility.ToJson(userProfile);

        databaseReference.Child("users").Child(userProfile.userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to save user profile with {task.Exception}");
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("User profile saved successfully.");
                currentUserProfile = userProfile;
                callback?.Invoke(true);
            }
        });
    }
    public void FetchUserProfile(string userId, Action<UserProfile> callback)
    {
        databaseReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to fetch user profile with {task.Exception}");
                callback?.Invoke(null); // Invoke callback with null if there's an error
                return;
            }

            if (task.Result != null && task.Result.Exists)
            {
                string userProfileJson = task.Result.GetRawJsonValue();
                Debug.Log(userProfileJson);
                UserProfile userProfile = JsonUtility.FromJson<UserProfile>(userProfileJson);
                //userType = userProfile.userType;
                callback?.Invoke(userProfile);
            }
            else
            {
                Debug.LogWarning("No user data found.");
                callback?.Invoke(null); // Invoke callback with null if no user data found
            }
        });
    }
    public void ForgotPassword(string email, Action<bool> callback)
    {
        loading.SetActive(true);
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Failed to send reset email. Check the email address.");
                callback?.Invoke(false);
                Debug.LogError("SendPasswordResetEmailAsync error: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                callback?.Invoke(true);
                Debug.Log("Reset email sent successfully. Check your email inbox.");
            }
            loading.SetActive(false);

        });
    }
    public static string GetErrorMessage(AggregateException exception)
    {
        Debug.Log(exception.ToString());
        FirebaseException firebaseEx = exception.InnerExceptions[0] as FirebaseException;

        if (firebaseEx != null)
        {
            var errorCode = (AuthError)firebaseEx.ErrorCode;
            return GetErrorMessage(errorCode);
        }

        return "An error ocurred! contact the developer";
    }
    private static string GetErrorMessage(AuthError errorCode)
    {
        var message = "";
        switch (errorCode)
        {
            case AuthError.AccountExistsWithDifferentCredentials:
                message = "Wrong username/password, check entered information!";
                break;
            case AuthError.MissingPassword:
                message = "Password is missing!";
                break;
            case AuthError.WeakPassword:
                message = "Password is weak, try to write stronger one!";
                break;

            case AuthError.EmailAlreadyInUse:
                message = "This emails is already exist!";
                break;
            case AuthError.InvalidEmail:
                message = "Invalid email!";
                break;
            case AuthError.MissingEmail:
                message = "Email is missing";
                break;
            default:
                message = "An error ocurred! contact the developer";
                break;

        }
        return message;
    }
    public void AddScore(int gems)
    {
        if (currentUserProfile == null)
        {
            Debug.LogError("User profile not loaded. Unable to update score.");
            return;
        }

        int newScore = currentUserProfile.Score + gems;
        currentUserProfile.Score = newScore;

        databaseReference.Child("users").Child(currentUserProfile.userId).Child("score")
            .SetValueAsync(newScore).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError($"Failed to update score: {task.Exception}");
                }
                else
                {
                    Debug.Log("Score updated successfully.");
                }
            });
    }
    public void FetchLeaderboard(System.Action<List<UserProfile>> callback)
    {
        databaseReference.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to fetch leaderboard data: {task.Exception}");
                callback(null);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                List<UserProfile> leaderboard = new List<UserProfile>();

                foreach (var child in snapshot.Children)
                {
                    UserProfile profile = JsonUtility.FromJson<UserProfile>(child.GetRawJsonValue());
                    leaderboard.Add(profile);
                }

                leaderboard = leaderboard.OrderByDescending(user => user.score).ToList();
                callback(leaderboard);
            }
        });
    }

    public void SaveLessonProgress(string userId, int level, int progress,int totalLessons)
    {
        var progressData = new Dictionary<string, object>
    {
        { "Level", level },
        { "Progress", progress },
        { "TotalLessons",totalLessons}
    };

        databaseReference.Child("lessons_progress").Child(userId).Child($"level_{level}").SetValueAsync(progressData)
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"Progress saved: Level {level}, Progress {progress}");
                }
                else
                {
                    Debug.LogError($"Failed to save progress: {task.Exception}");
                }
            });
    }
    public void FetchAllLessonProgress(string userId, System.Action<Dictionary<int, LessonProgress>> onProgressFetched)
    {
        databaseReference.Child("lessons_progress").Child(userId)
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("FetchAllLessonProgress task completed.");
                    DataSnapshot snapshot = task.Result;
                    Dictionary<int, LessonProgress> progress = new Dictionary<int, LessonProgress>();

                    if (snapshot.Exists)
                    {
                        //Debug.Log("Snapshot exists. Parsing data...");
                        foreach (var levelSnapshot in snapshot.Children)
                        {
                            Debug.Log($"Level Key: {levelSnapshot.Key}");
                            if (int.TryParse(levelSnapshot.Key.Replace("level_", ""), out int level) &&
                                levelSnapshot.HasChild("Progress"))
                            {
                                if (level != 1000) { 
                                    int wordsLearned = int.Parse(levelSnapshot.Child("Progress").Value.ToString());
                                    int totalLessons = int.Parse(levelSnapshot.Child("TotalLessons").Value.ToString());
                                    Debug.Log($"Parsed Level: {level}, Words Learned: {wordsLearned}");
                                    LessonProgress lesson = new LessonProgress();
                                    lesson.Progress=wordsLearned;
                                    lesson.TotalLessons=totalLessons;
                                    lesson.Level = level;
                                    progress[level] = lesson;
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"Failed to parse level or progress for key: {levelSnapshot.Key}");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("No data found in snapshot.");
                    }

                    //Debug.Log($"Final Progress Data: {string.Join(", ", progress.Select(kvp => $"Level {kvp.Key}: {kvp.Value}"))}");
                    //Debug.Log("Invoking onProgressFetched callback.");
                    onProgressFetched?.Invoke(progress);
                }
                else
                {
                    //Debug.LogError($"Failed to fetch all lesson progress: {task.Exception}");
                    onProgressFetched?.Invoke(null); // Return empty dictionary on failure
                }
            });
    }
    public void FetchAllExercizesMarks(string userId, System.Action<Dictionary<int, int>> onProgressFetched)
    {
        databaseReference.Child("exersizes_marks").Child(userId)
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    //Debug.Log("FetchAllLessonProgress task completed.");
                    DataSnapshot snapshot = task.Result;
                    Dictionary<int, int> progress = new Dictionary<int, int>();

                    if (snapshot.Exists)
                    {
                        foreach (var levelSnapshot in snapshot.Children)
                        {
                            if (int.TryParse(levelSnapshot.Key.Replace("exersize_", ""), out int level) &&
                                levelSnapshot.HasChild("mark"))
                            {
                                int wordsLearned = int.Parse(levelSnapshot.Child("mark").Value.ToString());
                                progress[level] = wordsLearned;
                            }
                            else
                            {
                                Debug.LogWarning($"Failed to parse level or progress for key: {levelSnapshot.Key}");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("No data found in snapshot.");
                    }

                    onProgressFetched?.Invoke(progress);
                }
                else
                {
                    //Debug.LogError($"Failed to fetch all lesson progress: {task.Exception}");
                    onProgressFetched?.Invoke(new Dictionary<int, int>()); // Return empty dictionary on failure
                }
            });
    }
    public void SaveExerciseProgress(string userId, int exercise, int mark)
    {
        var progressData = new Dictionary<string, object>
    {
        { "exercise", exercise },
        { "mark", mark }
    };

        databaseReference.Child("exersizes_marks").Child(userId).Child($"exersize_{exercise}").SetValueAsync(progressData)
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"Progress saved: Level {exercise}, Progress {mark}");
                }
                else
                {
                    Debug.LogError($"Failed to save progress: {task.Exception}");
                }
            });
    }

    public void SaveCategoryWeakness(string userId, List<CategoryWeakness> categoryWeakness)
    {
        foreach (var newWeakness in categoryWeakness)
        {
            // Check if the CategoryID already exists in the weaknessCategories list
            var existingWeakness = GameManager.Instance.weaknessCategories
                .FirstOrDefault(w => w.CategoryID == newWeakness.CategoryID);

            if (existingWeakness != null)
            {
                // If it exists, update the MeanWeakness by averaging the existing and new MeanWeakness
                existingWeakness.MeanWeakness = (existingWeakness.MeanWeakness + newWeakness.MeanWeakness) / 2;
            }
            else
            {
                // If it doesn't exist, add the new CategoryWeakness directly
                GameManager.Instance.weaknessCategories.Add(newWeakness);
            }
        }

        CategoryWeaknessList wrapper = new CategoryWeaknessList { weaknesses = GameManager.Instance.weaknessCategories };
        string data = JsonUtility.ToJson(wrapper);

        databaseReference.Child("category_weakness").Child(userId).SetRawJsonValueAsync(data)
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"New Categories added: {categoryWeakness.Count}");
                }
                else
                {
                    Debug.LogError($"Failed to save progress: {task.Exception}");
                }
            });
    }
    public void LoadCategoryWeakness(string userId,Action<List<CategoryWeakness>> callback)
    {
        databaseReference.Child("category_weakness").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to fetch leaderboard data: {task.Exception}");
            }
            else if (task.IsCompleted)
            {
                //DataSnapshot snapshot = task.Result;
                //List<CategoryWeakness> categories = new List<CategoryWeakness>();
                CategoryWeaknessList categories = JsonUtility.FromJson<CategoryWeaknessList>(task.Result.GetRawJsonValue());
                //foreach (var child in snapshot.Children)
                //{
                //    Debug.Log("Raw JSON: " + child.GetRawJsonValue());
                //    CategoryWeakness category = JsonUtility.FromJson<CategoryWeakness>(child.GetRawJsonValue());
                //    categories.Add(category);
                //}

                GameManager.Instance.weaknessCategories = categories.weaknesses.OrderByDescending(cateory => cateory.MeanWeakness).ToList();
                callback?.Invoke(GameManager.Instance.weaknessCategories);
                //.Clear();
                Debug.Log(categories.weaknesses.Count+" categories restored");
                //GameManager.Instance.weaknessCategories.AddRange(categories);
            }
        });

        
    }


    //public void FetchLessonProgress(string userId, Action<Dictionary<int, int>> callback)
    //{
    //    databaseReference.Child("users").Child(userId).Child("lessonProgress").GetValueAsync().ContinueWithOnMainThread(task =>
    //    {
    //        if (task.Exception != null)
    //        {
    //            Debug.LogError($"Failed to fetch lesson progress: {task.Exception}");
    //            callback?.Invoke(null);
    //            return;
    //        }

    //        if (task.Result.Exists)
    //        {
    //            Dictionary<int, int> progress = new Dictionary<int, int>();
    //            foreach (var child in task.Result.Children)
    //            {
    //                int level = int.Parse(child.Key);
    //                int wordsLearned = int.Parse(child.Value.ToString());
    //                progress[level] = wordsLearned;
    //            }
    //            callback?.Invoke(progress);
    //        }
    //        else
    //        {
    //            callback?.Invoke(new Dictionary<int, int>());
    //        }
    //    });
    //}
    public void FetchLessonProgress(string userId, int level, System.Action<int> onProgressFetched)
    {
        databaseReference.Child("lessons_progress").Child(userId).Child($"level_{level}")
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists && snapshot.HasChild("Progress"))
                    {

                        int progress = int.Parse(snapshot.Child("Progress").Value.ToString());
                        Debug.Log($"Fetched progress for Level {level}: {progress}");
                        onProgressFetched?.Invoke(progress); // Return the progress
                    }
                    else
                    {
                        Debug.Log($"No progress found for Level {level}, defaulting to 0.");
                        onProgressFetched?.Invoke(0); // Default to 0 if no progress exists
                    }
                }
                else
                {
                    Debug.LogError($"Failed to fetch progress for Level {level}: {task.Exception}");
                    onProgressFetched?.Invoke(0); // Default to 0 in case of an error
                }
            });
    }

    private void OnDisable()
    {
        if (isGuest)
            SignOut();
    }
    private void OnDestroy()
    {
        if (isGuest)
            SignOut();
    }
    [Serializable]
public class CategoryWeaknessList
    {
        public List<CategoryWeakness> weaknesses;
    }
    [Serializable]
    public class LessonProgress
    {
        public int Level;
        public int Progress;
        public int TotalLessons;
    }
}
