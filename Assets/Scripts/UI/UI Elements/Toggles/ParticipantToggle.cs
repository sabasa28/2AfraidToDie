using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ParticipantToggle : Clickable, IPointerUpHandler
{
    [SerializeField] UnityEvent OnClick;
    Toggle toggle;

    protected override void Awake()
    {
        base.Awake();

        toggle = GetComponent<Toggle>();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!toggle.interactable) return;

        OnClick?.Invoke();
    }
}