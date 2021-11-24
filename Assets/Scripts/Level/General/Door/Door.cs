using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] Animator animator = null;
    [SerializeField] DoorButton button = null;
    [SerializeField] GameObject closeDoorTrigger = null;
    [SerializeField] PlayerScreen[] playerScreens = null;

    [Header("Door properties")]
    [SerializeField] bool playerA = true;
    [SerializeField] bool isEntranceDoor = true;
    [SerializeField] int doorNumber = 0;
    bool isOpen = false;

    public PlayerScreen[] PlayerScreens { get { return playerScreens; } }

    public static event Action OnDoorUnlocked;
    public static event Action OnDoorOpen;
    public static event Action OnDoorClosed;
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

        if (isLocal) if (isEntranceDoor) OnTimerTriggered?.Invoke();
    } 

    public void Close(bool isLocal = true)
    {
        if (!isOpen) return;

        isOpen = false;
        animator.SetTrigger("Close");

        if (isLocal) OnDoorClosed?.Invoke();
    }

    public void EnableButton() => button.canBePressed = true;
}