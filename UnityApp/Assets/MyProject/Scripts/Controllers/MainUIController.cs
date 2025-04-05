using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainUIController : MonoBehaviour
{

    public void Logout() {
        FirebaseManager.Instance.SignOut();
    }
    public void LoadSceneAgain() {
        SceneManager.LoadScene("Main");
    }
}
