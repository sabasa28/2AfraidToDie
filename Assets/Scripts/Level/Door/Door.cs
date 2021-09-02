using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] DoorButton button = null;

    [SerializeField] bool isOpen = false;

    [SerializeField] Animator animator = null;
    [SerializeField] GameObject closeDoorTrigger;

    void Start()
    {
        if (isOpen) animator.SetTrigger("Open");
        else animator.SetTrigger("Close");
    }

    public void SwitchState()
    {
        if (isOpen) Close();
        else Open();
    }

    public void FixButton()
    {
        button.FixButton();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        animator.SetTrigger("Open");
        closeDoorTrigger.gameObject.SetActive(true);
    } 

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        animator.SetTrigger("Close");
    }
}