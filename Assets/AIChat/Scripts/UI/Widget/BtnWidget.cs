using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BtnWidget : MonoBehaviour 
{
    public Button btn;
    public Image backgroundImage;
    public Image decorateImage;
    public Text description;

    public void BindButton(UnityAction btnAction)
    {
        btn.onClick.AddListener(btnAction);
    }

    public void CleanListener()
    {
        btn.onClick.RemoveAllListeners();
    }

    public void SetBackgroundImage(string url)
    {
        if (backgroundImage == null) return;
        StartCoroutine(HttpHelper.Instance.HttpLoadSprite(url, delegate(Sprite sprite)
        {
            backgroundImage.sprite = sprite;
        }));
    }

    public void SetDecorateImage(string url)
    {
        if (decorateImage == null) return;
        StartCoroutine(HttpHelper.Instance.HttpLoadSprite(url, delegate(Sprite sprite)
        {
            decorateImage.sprite = sprite;
        }));
    }

    public void SetDescription(string text)
    {
        if (description == null) return;
        description.text = text;
    }

    public void SetButtonInteractable(bool value)
    {
        if (btn == null) return;
        btn.interactable = value;
    }

    public void ChangeBackgroundColor(Color color)
    {
        if (backgroundImage == null) return;
        backgroundImage.color = color;
    }
}
