using System;
using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [SerializeField] Door door;
    [SerializeField] bool canBePressed = true;
    [SerializeField] bool onlyOneUse = true;
    [SerializeField] Animator animator;
    static public event Action OnDoorOpen;
    static public event Action OnDoorClosed;

    public void OpenDoor()
    {
        door.Open();
        canBePressed = false;
        animator.SetTrigger("Press");
        OnDoorOpen?.Invoke();
    }

    public void CloseDoor()
    {
        door.Close();
        canBePressed = false;
        animator.SetTrigger("Press");

        OnDoorClosed?.Invoke();
    }
}