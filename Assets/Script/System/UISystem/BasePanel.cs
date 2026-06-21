using UnityEngine;

public abstract class BasePanel : MonoBehaviour
{
    public bool IsOpen { get; private set; }

    public void Open(object args = null)
    {
        if (IsOpen)
        {
            Refresh(args);
            return;
        }

        IsOpen = true;
        gameObject.SetActive(true);
        OnOpen(args);
    }

    public void Close()
    {
        if (!IsOpen) return;

        OnClose();
        IsOpen = false;
        gameObject.SetActive(false);
    }

    public void Refresh(object args = null)
    {
        OnRefresh(args);
    }

    protected virtual void OnOpen(object args) { }
    protected virtual void OnClose() { }
    protected virtual void OnRefresh(object args) { }
}
