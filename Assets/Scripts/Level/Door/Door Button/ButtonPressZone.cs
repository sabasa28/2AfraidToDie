using UnityEngine;

public class ButtonPressZone : Interactable
{
    [SerializeField] DoorButton doorButton = null;

    public override void OnClicked()
    {
        base.OnClicked();

        doorButton.OpenDoor();
    }
}