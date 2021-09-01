using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //----------
    // PA = Player A
    // PB = Player B
    //----------

    [HideInInspector] public bool playingAsPA;

    [Header("General References")]
    [SerializeField] Transform cameraTransform = null;
    [SerializeField] Text timeText = null;
    [SerializeField] Text instructionsText = null;

    [Header("Timer")]
    [SerializeField] float timerDuration = 20.0f;
    [SerializeField] float timerMistakeDecrease = 5.0f;
    bool timerOn = false;

    [Header("\"Spot the differences\" puzzle")]
    [SerializeField] Door paDoor = null;
    [SerializeField] Door pbDoor = null;
    [SerializeField] List<Interactable> paDifferences = null;
    [SerializeField] List<Interactable> pbDifferences = null;

    [SerializeField] PlayerMovementController movementController;

    Door door;
    List<Interactable> differences;
    public Action OnTimeEnd;
    public Action RespawnAtCheckpoint;

    Animator animator;

    Interactable hoveredInteractable;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void OnEnable()
    {
        DoorButton.OnDoorOpen += StartTimer;
    }

    void Start()
    {
        timeText.text = "Time: " + timerDuration;
        instructionsText.text = "Find all 3 differences between the two rooms to escape. If you make any mistakes, your timer will decrease!";
        StartCoroutine(EraseTextWithTimer(instructionsText, 10.0f));

        if (playingAsPA)
        {
            differences = paDifferences;
            door = paDoor;
        }
        else
        {
            differences = pbDifferences;
            door = pbDoor;
        }
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
                else if (differences.Contains(hoveredInteractable))
                {
                    differences.Remove(hoveredInteractable);
                    Debug.Log("difference selected. remaining differences: " + differences.Count);

                    if (differences.Count <= 0)
                    {
                        timerOn = false;
                        door.Open();
                        Debug.Log("you win");
                    }
                }
                else timerDuration -= timerMistakeDecrease;
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

    void OnDisable()
    {
        DoorButton.OnDoorOpen -= StartTimer;
    }

    void GrabObject(Interactable interactable)
    {
        interactable.OnPlayerWatching();
        interactable.transform.parent = cameraTransform;
    }

    void StartTimer()
    {
        timerOn = true;
        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        while (timerOn)
        {
            timeText.text = "Time: " + timerDuration;
            timerDuration -= Time.deltaTime;

            if (timerDuration <= 0.0f)
            {
                timerDuration = 0.0f;
                timeText.text = timerDuration.ToString();

                timerOn = false;
                Debug.Log("you lost");
                Fall();
            }

            yield return null;
        }
    }

    public void Fall()
    {
        movementController.setCharacterControllerActiveState(false);
        OnTimeEnd();
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