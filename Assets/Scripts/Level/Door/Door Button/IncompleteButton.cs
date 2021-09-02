using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncompleteButton : MonoBehaviour
{
    [SerializeField] ButtonMissingPart missingPart;
    [SerializeField] DoorButton doorButton;
    public bool TryFixButtonWithObj(GameObject insertedObject)
    {
        ButtonMissingPart temp;
        insertedObject.TryGetComponent(out temp);
        if (temp != missingPart) return false;
        doorButton.FixButton();
        temp.ResetState();
        return true;
    }
}
