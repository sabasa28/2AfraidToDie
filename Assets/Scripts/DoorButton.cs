using System;
using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [SerializeField] Door door = null;

    static public event Action OnDoorOpen;
    static public event Action OnDoorClosed;

    public void OpenDoor()
    {
        door.gameObject.SetActive(false);

        OnDoorOpen?.Invoke();
    }

    public void CloseDoor()
    {
        door.gameObject.SetActive(true);

        OnDoorClosed?.Invoke();
    }
}