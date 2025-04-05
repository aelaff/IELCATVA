using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesController : MonoBehaviour
{
    public float splashScreenDuration = 3f;
    public bool isSplashScreenShown = false;

    void Start()
    {
        if (isSplashScreenShown)
        {
            Invoke("LoadNextScene", splashScreenDuration);
        }
    }


    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (FirebaseManager.Instance.user != null)
        {
            SceneManager.LoadScene(currentSceneIndex + 2);

        }
        else {
            SceneManager.LoadScene(currentSceneIndex + 1);

        }

    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
