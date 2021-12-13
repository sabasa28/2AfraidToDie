using UnityEngine;
using UnityEngine.EventSystems;

public class Clickable : MonoBehaviour, IPointerDownHandler
{
    AudioManager audioManager;

    virtual protected void Awake() => audioManager = AudioManager.Get();

    public void OnPointerDown(PointerEventData eventData) => audioManager.PlayUISFX(AudioManager.UISFXs.Click);
}