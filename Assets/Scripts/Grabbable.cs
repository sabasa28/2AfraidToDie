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
        Animator animator;
        TryGetComponent(out animator);
        if (animator) animator.enabled = false;
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