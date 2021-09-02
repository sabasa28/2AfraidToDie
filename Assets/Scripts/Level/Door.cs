using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] DoorButton button = null;

    [SerializeField] bool isOpen = false;

    [SerializeField] Animator animator = null;

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
        //Debug.Log("door opened");
        animator.SetTrigger("Open");
    } 

    public void Close()
    {
        if (!isOpen) return;
        //Debug.Log("door closed");
        animator.SetTrigger("Close");
    }
}