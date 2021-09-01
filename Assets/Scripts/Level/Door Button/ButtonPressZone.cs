using UnityEngine;

public class ButtonPressZone : MonoBehaviour
{
    [SerializeField] DoorButton doorButton = null;
    [SerializeField] bool broken = false;
    public void Press()
    {
        if(!broken) doorButton.OpenDoor();
    }
}