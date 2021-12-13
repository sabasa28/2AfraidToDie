using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    //----------
    // PA = Player A
    // PB = Player B
    //----------

    public PlayerMovementController movementController = null;
    [SerializeField] Vector3 GrabbedObjectPos;
    [HideInInspector] Transform cameraTransform = null;

    Grabbable grabbedObject = null;

    Animator animator;

    [SerializeField] Interactable hoveredInteractable;

    public static event Action<GameObject> OnFixingButton;
    public static event Action OnRespawn;

    void Awake()
    {
        animator = GetComponent<Animator>();

        //if (photonView.IsMine)
        //{
            cameraTransform = GameObject.Find("Main Camera").transform;
            movementController.cameraTransform = cameraTransform;
        //}
    }

    void OnEnable()
    {
        Grabbable.OnGrabbed += GrabObject;
        IncompleteButton.OnTryingToBeFixed += FixButton;
        ShapeInsertionTube.OnTryToInsertShape += DeliverShape;
        Phone.OnTryToUsePhone += UsePhone;
    }

    void Update()
    {
        //Difference selection
        if (hoveredInteractable != null)
        {
            hoveredInteractable.OnPlayerHovering();

            if (Input.GetButtonDown("Click") && hoveredInteractable != null) hoveredInteractable.OnClicked();
        }
    }

    void FixedUpdate()
    {
        //if (!photonView.IsMine) return;
        /*else */if (GameplayController.Get().OnPause)
        {
            hoveredInteractable = null;
            return;
        }

        RaycastHit hit;
        Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 20.0f, (1 << LayerMask.NameToLayer("Interactable")) | (1 << LayerMask.NameToLayer("Walls")));

        if (hit.collider && hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
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
        Phone.OnTryToUsePhone -= UsePhone;
    }

    void GrabObject(Grabbable grabbable)
    {
        //if (!photonView.IsMine) return;

        grabbable.OnPlayerHovering();
        grabbable.transform.parent = cameraTransform;
        grabbable.transform.localPosition = GrabbedObjectPos;

        grabbedObject = grabbable;
    }

    void FixButton(DoorButton doorButton) { if (/*photonView.IsMine && */grabbedObject) doorButton.TryToFixButton(grabbedObject.gameObject); }

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

    void UsePhone(bool startUsing)
    {
        movementController.SetCursorLockState(!startUsing);
        movementController.ableToMove = !startUsing;
        movementController.SetRotationActiveState(!startUsing);
    }

    public void Fall()
    {
        movementController.SetCharacterControllerActiveState(false);
        animator.SetBool("Fall", true);
    }

    public void Respawn()
    {
        movementController.SetCharacterControllerActiveState(true);
        if (grabbedObject) Destroy(grabbedObject.gameObject);
        animator.SetBool("Fall", false);

        /*if (photonView.IsMine) */OnRespawn?.Invoke();
    }
}