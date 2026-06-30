using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonAudio : MonoBehaviour
{
    [SerializeField] private string audioName = GameAudioCatalog.ButtonClick;

    private Button _button;
    private bool _isBound;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void Reset()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        BindRuntime();
    }

    private void OnDisable()
    {
        Unbind();
    }

    public void BindRuntime()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_button == null)
        {
            return;
        }

        // 面板代码会频繁 RemoveAllListeners，这里每次显式重挂一次，避免内部状态与 Button 实际监听列表失步。
        _button.onClick.RemoveListener(HandleClick);
        _button.onClick.AddListener(HandleClick);
        _isBound = true;
    }

    private void Unbind()
    {
        if (_button == null || !_isBound)
        {
            return;
        }

        _button.onClick.RemoveListener(HandleClick);
        _isBound = false;
    }

    public void HandleClick()
    {
        if (_button == null || !_button.IsInteractable() || !enabled || !gameObject.activeInHierarchy)
        {
            return;
        }

        AudioManager.Instance?.Play(audioName, AudioTrack.UI);
    }
}
