using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    bool isGrabbed;

    public void SetGrabbedState(bool grabbedState)
    {
        isGrabbed = grabbedState;
        TrySetRB();
    }

    void TrySetRB()
    {
        Rigidbody rb;
        TryGetComponent(out rb);
        if (transform) rb.isKinematic = isGrabbed;
    }
}
