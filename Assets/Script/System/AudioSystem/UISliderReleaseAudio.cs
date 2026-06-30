using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UISliderReleaseAudio : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] private string audioName = GameAudioCatalog.ButtonClick;
    [SerializeField] private AudioTrack audioTrack = AudioTrack.UI;

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Reset()
    {
        _slider = GetComponent<Slider>();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_slider == null || !_slider.interactable)
        {
            return;
        }

        AudioManager.Instance?.Play(audioName, audioTrack);
    }
}
