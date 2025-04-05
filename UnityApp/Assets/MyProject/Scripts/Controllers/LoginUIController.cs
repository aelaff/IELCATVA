using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LoginUIController : MonoBehaviour
{

    // Registration UI elements
    public TMP_InputField registrationUsernameInput;
    public TMP_InputField registrationPasswordInput;
    public TMP_InputField registrationRepeatPasswordInput;
    public Button registerButton;
    public Toggle termsToggle;

    // Sign-in UI elements
    public TMP_InputField signInUsernameInput;
    public TMP_InputField signInPasswordInput;
    public Button signInButton;
    public Button signInGuest;


    public PopupController popupController;
    //private const string AppleUserIdKey = "AppleUserId";
    //public string webClientId = "752701637593-mkgmp0257fp1pqleej6dp3k1ot807fu9.apps.googleusercontent.com";

    //private GoogleSignInConfiguration configuration;
    //private IAppleAuthManager _appleAuthManager;

    private void Awake()
    {
        /*
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true
        };
        */

    }
    void Start()
    {
        
        
        registerButton.onClick.AddListener(RegisterUser);
        signInButton.onClick.AddListener(LoginUser);
        signInGuest.onClick.AddListener(LoginGuest);
        /*
        // If the current platform is supported
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
            var deserializer = new PayloadDeserializer();
            // Creates an Apple Authentication manager with the deserializer
            this._appleAuthManager = new AppleAuthManager(deserializer);
        }

        this.InitializeLoginMenu();
        */

    }

    private void LoginGuest()
    {
        FirebaseManager.Instance.SignInAnonymously();
    }
    /*
    public void OnGoogleSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
          OnAuthenticationFinished);
    }

    public void OnGoogleSignOut()
    {
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<System.Exception> enumerator =
                    task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error =
                            (GoogleSignIn.SignInException)enumerator.Current;
                    popupController.ShowPopup("Login Failed!", error.Message, false);

                }
                else
                {
                    popupController.ShowPopup("Login Failed!", "please try again later", false);

                }
            }
        }
        else if (task.IsCanceled)
        {
            popupController.ShowPopup("Login Failed!", "please try again later", false);
        }
        else
        {
            //AddStatusText("Welcome: " + task.Result.DisplayName + "!");
            FirebaseManager.Instance.FirebaseGoogleLogin(task.Result.IdToken,null);
        }
    }
    private void Update()
    {
        if (this._appleAuthManager != null)
        {
            this._appleAuthManager.Update();
        }
    }
    private void InitializeLoginMenu()
    {
        

        // Check if the current platform supports Sign In With Apple
        if (this._appleAuthManager == null)
        {
            return;
        }

        // If at any point we receive a credentials revoked notification, we delete the stored User ID, and go back to login
        this._appleAuthManager.SetCredentialsRevokedCallback(result =>
        {
            Debug.Log("Received revoked callback " + result);
            PlayerPrefs.DeleteKey(AppleUserIdKey);
        });

        // If we have an Apple User Id available, get the credential status for it
        if (PlayerPrefs.HasKey(AppleUserIdKey))
        {
            var storedAppleUserId = PlayerPrefs.GetString(AppleUserIdKey);
            this.CheckCredentialStatusForUserId(storedAppleUserId);
        }
        // If we do not have an stored Apple User Id, attempt a quick login
        else
        {
            //this.AttemptQuickLogin();
        }
    }
    

    private void SetupFirebaseData(string appleUserId, ICredential credential)
    {
        var appleIdCredential = credential as IAppleIDCredential;
        var yourCustomNonce = "RANDOM_NONCE_FORTHEAUTHORIZATIONREQUEST";
        var yourCustomState = "RANDOM_STATE_FORTHEAUTHORIZATIONREQUEST";

        var loginArgs = new AppleAuthLoginArgs(
            LoginOptions.IncludeEmail | LoginOptions.IncludeFullName,
            yourCustomNonce,
            yourCustomState);
        var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0, appleIdCredential.IdentityToken.Length);
        FirebaseAppleLogin(identityToken, yourCustomNonce);
        Debug.Log("Ahmed : " + appleUserId);
    }

    private void CheckCredentialStatusForUserId(string appleUserId)
    {
        // If there is an apple ID available, we should check the credential state
        this._appleAuthManager.GetCredentialState(
            appleUserId,
            state =>
            {
                switch (state)
                {
                    // If it's authorized, login with that user id
                    case CredentialState.Authorized:
                        this.SetupFirebaseData(appleUserId, null);
                        return;

                    // If it was revoked, or not found, we need a new sign in with apple attempt
                    // Discard previous apple user id
                    case CredentialState.Revoked:
                    case CredentialState.NotFound:
                        PlayerPrefs.DeleteKey(AppleUserIdKey);
                        return;
                }
            },
            error =>
            {
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Error while trying to get credential state " + authorizationErrorCode.ToString() + " " + error.ToString());
            });
    }

    private void AttemptQuickLogin()
    {
        var quickLoginArgs = new AppleAuthQuickLoginArgs();

        // Quick login should succeed if the credential was authorized before and not revoked
        this._appleAuthManager.QuickLogin(
            quickLoginArgs,
            credential =>
            {
                // If it's an Apple credential, save the user ID, for later logins
                var appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential != null)
                {
                    PlayerPrefs.SetString(AppleUserIdKey, credential.User);
                }

                this.SetupFirebaseData(credential.User, credential);
            },
            error =>
            {
                // If Quick Login fails, we should show the normal sign in with apple menu, to allow for a normal Sign In with apple
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Quick Login Failed " + authorizationErrorCode.ToString() + " " + error.ToString());
            });
    }
    public void SignInWithAppleButtonPressed()
    {
        this.SignInWithApple();
    }
    private void SignInWithApple()
    {
        if (this._appleAuthManager == null)
        {
            popupController.ShowPopup("Apple sign in not supported in this platform !", "", false);
            return;
        }
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        this._appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                // If a sign in with apple succeeds, we should have obtained the credential with the user id, name, and email, save it
                PlayerPrefs.SetString(AppleUserIdKey, credential.User);
                this.SetupFirebaseData(credential.User, credential);
            },
            error =>
            {
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " + error.ToString());
            });
    }

    */
    private void OnEnable()
    {
        FirebaseManager.OnDisplayErrorMessage += DisplayErrorMessage;
    }

    private void OnDisable()
    {
        FirebaseManager.OnDisplayErrorMessage -= DisplayErrorMessage;
    }

    private void DisplayErrorMessage(string title, string message, bool isSuccess)
    {
        popupController.ShowPopup(title, message, isSuccess);
    }


    void RegisterUser()
    {
        string username = registrationUsernameInput.text;
        string password = registrationPasswordInput.text;
        string repeatPassword = registrationRepeatPasswordInput.text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(repeatPassword))
        {
            popupController.ShowPopup("Please fill in all fields!", "", false);
            return;
        }

        if (!IsValidEmail(username))
        {
            popupController.ShowPopup("Invalid email format!", "", false);
            return;
        }

        if (password != repeatPassword)
        {
            popupController.ShowPopup("Passwords do not match!", "", false);
            return;
        }
        if (!termsToggle.isOn)
        {
            popupController.ShowPopup("Please accept the terms!", "", false);
            return;
        }
        FirebaseManager.Instance.RegisterUser(username, password);


    }

    void LoginUser()
    {
        string username = signInUsernameInput.text;
        string password = signInPasswordInput.text;
        if (!IsValidEmail(username))
        {
            popupController.ShowPopup("Invalid email format!", "", false);
            return;
        }
        else { 
            FirebaseManager.Instance.Login(username, password);
            
        }
    }

    bool IsValidEmail(string email)
    {
        string pattern = @"^[\w.-]+@[a-zA-Z\d.-]+\.[a-zA-Z]{2,4}$";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(email);
    }
    
    public void FirebaseGoogleLogin(string googleIdToken, string googleAccessToken)
    {
        FirebaseManager.Instance.FirebaseGoogleLogin(googleIdToken, googleAccessToken);
    }

    public void FirebaseAppleLogin(string appleIdToken, string rawNonce)
    {
        FirebaseManager.Instance.FirebaseAppleLogin(appleIdToken, rawNonce);
    }
    
    public void SignOut()
    {
        FirebaseManager.Instance.SignOut();
    }


}



