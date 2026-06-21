using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public string battleSceneName = "Demo";
    public Button startButton;
    public Button quitButton;

    private void Awake()
    {
        if (startButton == null)
        {
            GameObject target = GameObject.Find("Button_StartBattle");
            if (target != null) startButton = target.GetComponent<Button>();
        }

        if (quitButton == null)
        {
            GameObject target = GameObject.Find("Button_QuitGame");
            if (target != null) quitButton = target.GetComponent<Button>();
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartBattle);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    public void StartBattle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(battleSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
