using System;
using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [SerializeField] Door door = null;
    [SerializeField] ButtonPressZone pressZone = null;

    [SerializeField] bool onlyOneUse = true;
    [SerializeField] bool broken = false;
    public bool canBePressed = true;

    [SerializeField] Animator animator = null;
    [SerializeField] ButtonMissingPart missingPart = null;

    static public event Action OnDoorUnlocked;

    void OnEnable() => Player.OnFixingButton += TryToFixButton;

    void Start() { if (broken) pressZone.gameObject.SetActive(false); }

    void OnDisable() => Player.OnFixingButton -= TryToFixButton;

    public void TryToFixButton(GameObject insertedObject)
    {
        ButtonMissingPart temp;
        insertedObject.TryGetComponent(out temp);
        
        if (temp == null) return;
        
        FixButton();
        temp.ResetState();
    }

    public void FixButton()
    {
        broken = false;
        pressZone.gameObject.SetActive(true);
    }

    public void UnlockDoor()
    {
        canBePressed = false;
        animator.SetTrigger("Press");

        door.Unlock();
    }

    public void OpenDoor() => door.Open();

    public void CloseDoor()
    {
        canBePressed = false;
        animator.SetTrigger("Press");

        door.Close();
    }
}