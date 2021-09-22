using System;
using UnityEngine;

public class ShapeInsertionTube : Interactable
{
    [SerializeField] DeliveryMachine deliveryMachine;

    public static event Action<DeliveryMachine> OnTryToInsertShape;

    public override void OnClicked()
    {
        base.OnClicked();

        OnTryToInsertShape?.Invoke(deliveryMachine);
    }

}
