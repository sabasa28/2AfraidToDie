using UnityEngine;

public class IncompleteButton : MonoBehaviour
{
    [SerializeField] ButtonMissingPart missingPart = null;
    [SerializeField] DoorButton doorButton = null;
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