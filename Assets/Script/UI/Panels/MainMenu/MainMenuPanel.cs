using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : BasePanel
{
    public Button startButton;
    public Button optionsButton;
    public Button cloudSaveButton;
    public Button quitButton;

    protected override void OnOpen(object args)
    {
        EnsureButtonReferences();
        BindButtons();
    }

    protected override void OnClose()
    {
        UnbindButtons();
    }

    private void BindButtons()
    {
        UnbindButtons();

        if (startButton != null) startButton.onClick.AddListener(HandleStart);
        if (optionsButton != null) optionsButton.onClick.AddListener(HandleOptions);
        if (cloudSaveButton != null) cloudSaveButton.onClick.AddListener(HandleCloudSave);
        if (quitButton != null) quitButton.onClick.AddListener(HandleQuit);
    }

    private void UnbindButtons()
    {
        if (startButton != null) startButton.onClick.RemoveAllListeners();
        if (optionsButton != null) optionsButton.onClick.RemoveAllListeners();
        if (cloudSaveButton != null) cloudSaveButton.onClick.RemoveAllListeners();
        if (quitButton != null) quitButton.onClick.RemoveAllListeners();
    }

    private void HandleStart()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ClosePanel<MainMenuPanel>();
            UIManager.Instance.OpenPanel<CharacterSelectPanel>("UI/Panels/CharacterSelect", UILayer.Panel);
        }
        else
        {
            Debug.LogWarning("[MainMenuPanel] 找不到 UIManager。");
        }
    }

    private void HandleOptions()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenPanel<MainMenuSettingsPanel>("UI/Panels/MainMenuSettings", UILayer.Popup);
        }
    }

    private void HandleCloudSave()
    {
        Debug.Log("[MainMenuPanel] Cloud Save reserved.");
    }

    private void HandleQuit()
    {
        Application.Quit();
    }

    private void EnsureButtonReferences()
    {
        if (startButton == null) startButton = FindButton("Button_Start");
        if (optionsButton == null) optionsButton = FindButton("Button_Options");
        if (cloudSaveButton == null) cloudSaveButton = FindButton("Button_CloudSave");
        if (quitButton == null) quitButton = FindButton("Button_Quit");
    }

    private Button FindButton(string nodeName)
    {
        Transform child = transform.Find(nodeName);
        return child != null ? child.GetComponent<Button>() : null;
    }
}
