using System;
using UnityEngine;

public class IncompleteButton : Interactable
{
    [SerializeField] ButtonMissingPart missingPart = null;
    [SerializeField] DoorButton doorButton = null;

    public static event Action<DoorButton> OnTryingToBeFixed;

    public override void OnClicked()
    {
        base.OnClicked();

        OnTryingToBeFixed?.Invoke(doorButton);
    }
}