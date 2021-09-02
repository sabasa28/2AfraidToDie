using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //----------
    // PA = Player A
    // PB = Player B
    //----------

    [SerializeField] Transform cameraTransform = null;
    [SerializeField] Text instructionsText = null;
    [SerializeField] PlayerMovementController movementController = null;

    public Action RespawnAtCheckpoint;

    Animator animator;

    Interactable hoveredInteractable;

    static public event Action<Interactable> OnDifferenceObjectSelected;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        instructionsText.text = "Find all 3 differences between the two rooms to escape. If you make any mistakes, your timer will decrease!";
        StartCoroutine(EraseTextWithTimer(instructionsText, 10.0f));
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

    IEnumerator EraseTextWithTimer(Text text, float time)
    {
        yield return new WaitForSeconds(time);
        text.text = "";
    }
}