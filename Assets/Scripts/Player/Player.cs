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

    Grabbable grabbedObject = null;

    Animator animator;

    [SerializeField] Interactable hoveredInteractable;

    public Action RespawnAtCheckpoint;
    public static event Action<GameObject> OnFixingButton;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        Grabbable.OnGrabbed += GrabObject;
        IncompleteButton.OnTryingToBeFixed += FixButton;
        ShapeInsertionTube.OnTryToInsertShape += DeliverShape;
    }

    void Update()
    {
        //Difference selection
        if (hoveredInteractable != null)
        {
            hoveredInteractable.OnPlayerHovering();

            if (Input.GetButtonDown("Click") && hoveredInteractable != null) hoveredInteractable.OnClicked();
            //{
            //
            //    //if (hoveredInteractable.CompareTag("Door Button")) hoveredInteractable.GetComponent<ButtonPressZone>().Press();
            //    //else if (hoveredInteractable.CompareTag("Difference Object")) OnDifferenceObjectSelected?.Invoke(hoveredInteractable);
            //    //else if (hoveredInteractable.CompareTag("Grabbable")) GrabObject(hoveredInteractable);
            //    //else if (objectGrabbed && hoveredInteractable.CompareTag("IncompleteButton")) hoveredInteractable.GetComponent<IncompleteButton>().TryFixButtonWithObj(objectGrabbed.gameObject);
            //}
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

            if (hoveredInteractable != null)
            {
                if (hitInteractable != hoveredInteractable)
                {
                    hoveredInteractable.OnPlayerNotHovering();

                    hoveredInteractable = hitInteractable;
                    hoveredInteractable.OnPlayerHovering();
                }
            }
            else
            {
                hoveredInteractable = hitInteractable;
                hoveredInteractable.OnPlayerHovering();
            }
        }
        else if (hoveredInteractable != null)
        {
            hoveredInteractable.OnPlayerNotHovering();
            hoveredInteractable = null;
        }

        Debug.DrawRay(cameraTransform.position, cameraTransform.forward * 10.0f, Color.red);
    }

    void OnDisable()
    {
        Grabbable.OnGrabbed -= GrabObject;
        IncompleteButton.OnTryingToBeFixed -= FixButton;
        ShapeInsertionTube.OnTryToInsertShape -= DeliverShape;
    }

    void GrabObject(Grabbable grabbable)
    {
        grabbable.OnPlayerHovering();
        grabbable.transform.parent = cameraTransform;
        grabbable.transform.localPosition = GrabbedObjectPos;

        grabbedObject = grabbable.GetComponent<Grabbable>();
        //grabbedObject.SetGrabbedState(true);
        //grabbedObject.transform.localPosition = GrabbedObjectPos;
    }

    void FixButton(DoorButton doorButton)
    {
        doorButton.TryToFixButton(grabbedObject.gameObject);
    }

    void DeliverShape(DeliveryMachine deliveryMachine)
    {
        if (!grabbedObject) return;
        CreatedShape carriedShape = null;
        grabbedObject.TryGetComponent(out carriedShape);
        if (carriedShape)
        {
            deliveryMachine.InsertShape(carriedShape);
        }
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