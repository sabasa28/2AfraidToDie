using UnityEngine;

public class ButtonPressZone : Interactable
{
    [SerializeField] DoorButton button = null;

    public override void OnPlayerHovering()
    {
        if (!button.canBePressed) return;

        base.OnPlayerHovering();
    }

    public override void OnClicked()
    {
        if (!button.canBePressed) return;

        base.OnClicked();

        button.UnlockDoor();
    }
}