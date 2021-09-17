using System;
using UnityEngine;

public class Grabbable : Interactable
{
    bool isGrabbed;

    public static event Action<Grabbable> OnGrabbed;

    void TrySetRB()
    {
        Rigidbody rb;
        TryGetComponent(out rb);
        if (transform) rb.isKinematic = isGrabbed;
    }

    public override void OnClicked()
    {
        base.OnClicked();

        SetGrabbedState(true);
        OnGrabbed?.Invoke(this);
    }

    public void SetGrabbedState(bool grabbedState)
    {
        isGrabbed = grabbedState;
        TrySetRB();
    }
}