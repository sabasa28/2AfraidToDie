using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public enum MovementToDo
    { 
        riseAndLeave,
    }
    MovementToDo startingMovement;
    [SerializeField] Animator animator;
    public void SetStartingMovement(MovementToDo movementToDo)
    {
        switch (startingMovement)
        {
        case MovementToDo.riseAndLeave:
                animator.SetTrigger("Leave");
            break;
        }
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }
}
