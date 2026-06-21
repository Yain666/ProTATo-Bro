using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconSlot : MonoBehaviour
{
    public Image backgroundImage;
    public Image iconImage;

    public void SetBackgroundImage(Sprite sprite)
    {
        if (backgroundImage == null) return;
        backgroundImage.sprite = sprite;
    }

    public void SetIconImage(Sprite sprite)
    {
        if (iconImage == null) return;
        iconImage.sprite = sprite;
    }

    public void SetIconImageActive(bool active)
    {
        if (iconImage == null) return;
        iconImage.gameObject.SetActive(active);
    }
}
