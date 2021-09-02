using UnityEngine;

public class ButtonPressZone : MonoBehaviour
{
    [SerializeField] DoorButton doorButton = null;

    public void Press()
    {
        doorButton.OpenDoor();
    }
}