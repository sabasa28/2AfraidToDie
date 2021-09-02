using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonMissingPart : MonoBehaviour
{
    Vector3 origPos;

    private void Awake()
    {
        origPos = transform.position;
    }

    public void ResetState()
    {
        transform.position = origPos;
        gameObject.SetActive(false);
        transform.parent = null;
    }
}
