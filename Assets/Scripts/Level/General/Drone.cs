using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public enum MovementToDo
    { 
        riseAndLeave,
        deliverPulser,
    }
    MovementToDo startingMovement;
    [SerializeField] Animator animator;
    ButtonMissingPart pulser;
    [SerializeField] ButtonMissingPart pulserPrefab;
    [SerializeField] Transform pulserTransformations;
    int grabbedPulser;
    public void SetStartingMovement(MovementToDo movementToDo)
    {
        startingMovement = movementToDo;
        switch (startingMovement)
        {
        case MovementToDo.riseAndLeave:
                animator.SetTrigger("Leave");
            break;
        case MovementToDo.deliverPulser:
                CarryPulser();
                animator.SetTrigger("Deliver");
            break;
        }
    }

    void CarryPulser()
    {
        pulser = Instantiate(pulserPrefab, transform);
        pulser.transform.localPosition = pulserTransformations.position;
        pulser.transform.localRotation = pulserTransformations.rotation;
        pulser.transform.localScale = pulserTransformations.localScale;
        pulser.GetComponent<Rigidbody>().isKinematic = true;
    }

    void LetGoOfPulser()
    {
        pulser.transform.parent = null;
        pulser.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void Despawn()
    {
        gameObject.SetActive(false);
    }

    public ButtonMissingPart GetPulser()
    {
        return pulser;
    }
}
