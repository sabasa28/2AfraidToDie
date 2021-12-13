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

    [Header("Audio")]
    [SerializeField] AudioSource audioSource = null;
    [Space]
    [SerializeField] AudioClip openCloseSFX;

    public bool IsEntranceDoor { get { return isEntranceDoor; } }
    public PlayerScreen[] PlayerScreens { get { return playerScreens; } }

    public static event Action OnDoorUnlocked;
    public static event Action OnExitDoorUnlocked;
    static public event Action OnEntranceDoorOpen;
    public static event Action OnDoorClosed;

    void Start()
    {
        if (isOpen) animator.SetTrigger("Open");
        else animator.SetTrigger("Close");

        audioSource.clip = openCloseSFX;
    }

    void ResetPlayerScreens() { foreach (PlayerScreen screen in playerScreens) screen.On = false; }

    public void SwitchState()
    {
        if (isOpen) Close();
        else Open();
    }

    public void FixButton() => button.FixButton();

    public void Unlock()
    {
        if (!isEntranceDoor) OnExitDoorUnlocked?.Invoke();
        OnDoorUnlocked.Invoke();
    }

    public void Open(bool isLocal = true)
    {
        if (isOpen) return;

        isOpen = true;
        animator.SetTrigger("Open");
        closeDoorTrigger.gameObject.SetActive(true);

        audioSource.Play();

        if (isLocal && isEntranceDoor) OnEntranceDoorOpen?.Invoke();
    } 

    public void Close(bool isLocal = true)
    {
        if (!isOpen) return;

        isOpen = false;
        animator.SetTrigger("Close");
        ResetPlayerScreens();

        audioSource.Play();

        if (isLocal) OnDoorClosed?.Invoke();
    }

    public void EnableButton() => button.canBePressed = true;
}