using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPressZone : MonoBehaviour
{
    [SerializeField] DoorButton doorButton;
    [SerializeField] bool broken = false;
    public void Press()
    {
        if(!broken) doorButton.OpenDoor();
    }
}
