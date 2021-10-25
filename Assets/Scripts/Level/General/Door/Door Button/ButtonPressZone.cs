using UnityEngine;

public class ButtonPressZone : Interactable
{
    [SerializeField] DoorButton button = null;

    public override void OnClicked()
    {
        if (!button.canBePressed) return;

        base.OnClicked();

        button.OpenDoor();
    }
}