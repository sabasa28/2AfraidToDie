using System;
using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [SerializeField] Door door = null;
    [SerializeField] ButtonPressZone pressZone = null;

    [SerializeField] bool triggersTimer = false;
    [SerializeField] bool onlyOneUse = true;
    [SerializeField] bool broken = false;
    public bool canBePressed = true;

    [SerializeField] Animator animator = null;
    [SerializeField] ButtonMissingPart missingPart = null;

    static public event Action OnTimerTriggered;
    static public event Action OnDoorOpen;
    static public event Action OnDoorClosed;

    void OnEnable() => Player.OnFixingButton += TryToFixButton;

    void Start() { if (broken) pressZone.gameObject.SetActive(false); }

    void OnDisable() => Player.OnFixingButton -= TryToFixButton;

    public void TryToFixButton(GameObject insertedObject)
    {
        ButtonMissingPart temp;
        insertedObject.TryGetComponent(out temp);
        
        if (temp != missingPart) return;
        
        FixButton();
        temp.ResetState();
    }

    public void FixButton()
    {
        broken = false;
        pressZone.gameObject.SetActive(true);
    }

    public void OpenDoor()
    {
        door.Open();
        canBePressed = false;
        animator.SetTrigger("Press");

        OnDoorOpen?.Invoke();
        if (triggersTimer) OnTimerTriggered?.Invoke();
    }

    public void CloseDoor()
    {
        door.Close();
        canBePressed = false;
        animator.SetTrigger("Press");

        OnDoorClosed?.Invoke();
    }
}