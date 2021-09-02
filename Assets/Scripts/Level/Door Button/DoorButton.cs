using System;
using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [SerializeField] Door door = null;
    [SerializeField] ButtonPressZone pressZone = null;

    [SerializeField] bool triggersTimer = false;
    [SerializeField] bool canBePressed = true;
    [SerializeField] bool onlyOneUse = true;
    [SerializeField] bool broken = false;

    [SerializeField] Animator animator = null;

    static public event Action OnTimerTriggered;
    static public event Action OnDoorOpen;
    static public event Action OnDoorClosed;

    void Start()
    {
        if (broken) pressZone.gameObject.SetActive(false);
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