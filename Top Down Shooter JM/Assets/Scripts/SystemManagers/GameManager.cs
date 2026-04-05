using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;


    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void LoadScene(Scene targetscene)
    {
        SceneManager.LoadScene(targetscene.ToString());
    }

    void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.ToString());
    }

    void ExitGame()
    {
        Debug.Log("Exiting game");
        Application.Quit();
    }
}
