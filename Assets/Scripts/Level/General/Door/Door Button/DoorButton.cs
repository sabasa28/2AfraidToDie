using System;
using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [SerializeField] Door door = null;
    [SerializeField] ButtonPressZone pressZone = null;

    [SerializeField] bool broken = false;
    public bool canBePressed = true;

    [SerializeField] Animator animator = null;
    [SerializeField] ButtonMissingPart missingPart = null;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource = null;
    [Space]
    [SerializeField] AudioClip pressSFX = null;

    static public event Action OnDoorUnlocked;

    void OnEnable()
    {
        Player.OnFixingButton += TryToFixButton;
        Player.OnRespawn += ResetPressZone;
    }

    void Start()
    {
        if (broken) pressZone.gameObject.SetActive(false);

        audioSource.clip = pressSFX;
    }

    void OnDisable()
    {
        Player.OnFixingButton -= TryToFixButton;
        Player.OnRespawn -= ResetPressZone;
    }

    void ResetPressZone() { if (!door.IsEntranceDoor && pressZone.gameObject.activeInHierarchy) pressZone.gameObject.SetActive(false); }

    public void TryToFixButton(GameObject insertedObject)
    {
        if (!insertedObject.TryGetComponent(out ButtonMissingPart missingPart)) return;
        
        FixButton();
        missingPart.ResetState();
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

        audioSource.Play();

        door.Unlock();
    }

    public void CloseDoor()
    {
        animator.SetTrigger("Press");

        door.Close();
    }
}