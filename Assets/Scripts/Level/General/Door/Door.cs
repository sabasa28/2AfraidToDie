using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] Animator animator = null;
    [SerializeField] DoorButton button = null;
    [SerializeField] GameObject closeDoorTrigger = null;

    [Header("Door properties")]
    [SerializeField] bool playerA = true;
    [SerializeField] bool isEntranceDoor = true;
    [SerializeField] int doorNumber = 0;
    bool isOpen = false;

    public static event Action OnDoorUnlocked;
    public static event Action<bool, int> OnDoorOpen;
    public static event Action<bool, int> OnDoorClosed;
    static public event Action OnTimerTriggered;

    void Start()
    {
        if (isOpen) animator.SetTrigger("Open");
        else animator.SetTrigger("Close");
    }

    public void SwitchState()
    {
        if (isOpen) Close();
        else Open();
    }

    public void FixButton() => button.FixButton();

    public void Unlock() => OnDoorUnlocked.Invoke();

    public void Open(bool isLocal = true)
    {
        if (isOpen) return;

        isOpen = true;
        animator.SetTrigger("Open");
        closeDoorTrigger.gameObject.SetActive(true);

        if (isEntranceDoor) OnTimerTriggered?.Invoke();

        if (isLocal) OnDoorOpen?.Invoke(playerA, doorNumber);
    } 

    public void Close(bool isLocal = true)
    {
        if (!isOpen) return;

        isOpen = false;
        animator.SetTrigger("Close");

        if (isLocal) OnDoorClosed?.Invoke(playerA, doorNumber);
    }
}