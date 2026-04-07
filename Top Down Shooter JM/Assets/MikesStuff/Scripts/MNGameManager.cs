using UnityEngine;
using UnityEngine.SceneManagement;

public class MNGameManager : MonoBehaviour
{
    public static MNGameManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}