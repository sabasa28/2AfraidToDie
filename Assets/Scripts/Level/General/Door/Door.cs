using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] Animator animator = null;
    [SerializeField] DoorButton button = null;
    [SerializeField] GameObject closeDoorTrigger = null;

    [Header("Door properties")]
    [SerializeField] bool playerA = true;
    [SerializeField] int doorNumber = 0;

    [Space]
    [SerializeField] bool isExitDoor = false;
    [SerializeField] bool isOpen = false;

    public bool IsExitDoor { get { return isExitDoor; } }
    public bool IsLocked { set { button.canBePressed = !value; } get { return !button.canBePressed; } }

    public static Action<bool, int> OnDoorOpen;
    public static Action<bool, int> OnDoorClosed;

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

    public void Open(bool isLocal = true)
    {
        if (isOpen) return;

        isOpen = true;
        animator.SetTrigger("Open");
        closeDoorTrigger.gameObject.SetActive(true);

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