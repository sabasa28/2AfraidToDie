using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ParticipantToggle : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] UnityEvent OnClick;
    Toggle toggle;

    void Awake() => toggle = GetComponent<Toggle>();

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!toggle.interactable) return;

        OnClick?.Invoke();
    }
}