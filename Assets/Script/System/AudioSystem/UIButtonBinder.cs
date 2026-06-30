using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UIButtonBinder
{
    public static void Bind(Button button, UnityAction action)
    {
        if (button == null || action == null)
        {
            return;
        }

        UIButtonAudio audio = button.GetComponent<UIButtonAudio>();
        if (audio != null)
        {
            button.onClick.AddListener(audio.HandleClick);
        }

        button.onClick.AddListener(action);
    }

    public static void Bind(Button button, UnityAction primaryAction, UnityAction secondaryAction)
    {
        if (button == null)
        {
            return;
        }

        UIButtonAudio audio = button.GetComponent<UIButtonAudio>();
        if (audio != null)
        {
            button.onClick.AddListener(audio.HandleClick);
        }

        if (primaryAction != null)
        {
            button.onClick.AddListener(primaryAction);
        }

        if (secondaryAction != null)
        {
            button.onClick.AddListener(secondaryAction);
        }
    }
}
