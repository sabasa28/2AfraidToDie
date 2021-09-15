using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    //----------
    // PA = Player A
    // PB = Player B
    //----------

    [SerializeField] Transform cameraTransform = null;
    [SerializeField] Vector3 GrabbedObjectPos;
    public PlayerMovementController movementController = null;

    Grabbable objectGrabbed = null;

    public Action RespawnAtCheckpoint;

    Animator animator;

    [SerializeField]Interactable hoveredInteractable;

    public static event Action<Interactable> OnDifferenceObjectSelected;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        //Difference selection
        if (hoveredInteractable)
        {
            hoveredInteractable.OnPlayerWatching();

            if (Input.GetButtonDown("Click") && hoveredInteractable)
            {
                hoveredInteractable.OnClicked();

                if (hoveredInteractable.CompareTag("Door Button")) hoveredInteractable.GetComponent<ButtonPressZone>().Press();
                else if (hoveredInteractable.CompareTag("Difference Object")) OnDifferenceObjectSelected?.Invoke(hoveredInteractable);
                else if (hoveredInteractable.CompareTag("Grabbable")) GrabObject(hoveredInteractable);
                else if (objectGrabbed && hoveredInteractable.CompareTag("IncompleteButton")) hoveredInteractable.GetComponent<IncompleteButton>().TryFixButtonWithObj(objectGrabbed.gameObject);
            }
        }
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 20.0f, 1 << LayerMask.NameToLayer("Interactable"));

        if (hit.collider)
        {
            Interactable hitInteractable;
            hit.collider.TryGetComponent(out hitInteractable);

            if (hoveredInteractable)
            {
                if (hitInteractable != hoveredInteractable)
                {
                    hoveredInteractable.OnPlayerNotWatching();

                    hoveredInteractable = hitInteractable;
                    hoveredInteractable.OnPlayerWatching();
                }
            }
            else
            {
                hoveredInteractable = hitInteractable;
                hoveredInteractable.OnPlayerWatching();
            }
        }
        else if (hoveredInteractable)
        {
            hoveredInteractable.OnPlayerNotWatching();
            hoveredInteractable = null;
        }

        Debug.DrawRay(cameraTransform.position, cameraTransform.forward * 10.0f, Color.red);
    }

    void GrabObject(Interactable interactable)
    {
        interactable.OnPlayerWatching();
        interactable.transform.parent = cameraTransform;
        objectGrabbed = interactable.GetComponent<Grabbable>();
        objectGrabbed.SetGrabbedState(true);
        objectGrabbed.transform.localPosition = GrabbedObjectPos;
    }

    public void Fall()
    {
        movementController.setCharacterControllerActiveState(false);
        animator.SetBool("Fall", true);
    }

    public void Respawn()
    {
        animator.SetBool("Fall", false);
        RespawnAtCheckpoint();
        movementController.setCharacterControllerActiveState(true);
    }
}