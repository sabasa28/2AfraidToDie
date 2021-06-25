﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool playingAsPA;

    [Header("General References")]
    [SerializeField] Transform cameraTransform = null;
    [SerializeField] Transform groundCheck = null;
    [SerializeField] Text timeText = null;
    [SerializeField] Text instructionsText = null;
    CharacterController cc;
    
    [Header("Movement")] 
    [SerializeField] float movementSpeed = 0.0f;
    [SerializeField] float cameraSpeed = 0.0f;
    float camRotX;
    float lastCameraDirX;
    float lastCameraDirY;

    [Header("Physics")]
    [SerializeField] float gravityForce = 0.0f;
    [SerializeField] bool isGrounded;
    [SerializeField] float groundCheckRadius = 0.0f;
    [SerializeField] LayerMask walkableLayer = 0;
    Vector3 velocity;

    [Header("Timer")]
    [SerializeField] float timerDuration = 20.0f;
    [SerializeField] float timerMistakeDecrease = 5.0f;
    bool timerOn = false;

    [Header("\"Spot the differences\" puzzle")]
    [SerializeField] Door paDoor = null;
    [SerializeField] Door pbDoor = null;
    [SerializeField] List<Interactable> paDifferences = null;
    [SerializeField] List<Interactable> pbDifferences = null;
    Door door;
    List<Interactable> differences;

    Interactable hoveredInteractable;
    Material mat;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        cc.enabled = false;
    }

    void OnEnable()
    {
        DoorButton.OnDoorOpen += StartTimer;
    }

    void Start()
    {
        cc.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mat = GetComponent<MeshRenderer>().material;

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
        //Mouse input
        Vector3 viewRotation;
        viewRotation = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")) * cameraSpeed;

        cc.transform.Rotate(new Vector3(0, 1, 0), viewRotation.y * Time.deltaTime);
        camRotX += viewRotation.x * Time.deltaTime;
        camRotX = Mathf.Clamp(camRotX, -90.0f, 90.0f);
        cameraTransform.localRotation = Quaternion.Euler(camRotX, cameraTransform.localRotation.eulerAngles.y, 0);

        //Keyboard input
        float inputX = -Input.GetAxis("Vertical");
        float inputZ = Input.GetAxis("Horizontal");
        Vector3 movement;
        movement = (inputX*transform.right + inputZ * transform.forward) * movementSpeed;
        cc.Move(movement * Time.deltaTime);

        //Physics
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, walkableLayer);
        
        if (isGrounded)
            velocity = Vector3.zero;
        else
            velocity.y += gravityForce * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        //Difference selection
        if (hoveredInteractable)
        {
            hoveredInteractable.OnPlayerWatching();

            if (Input.GetButtonDown("Click") && hoveredInteractable)
            {
                hoveredInteractable.OnClicked();

                if (hoveredInteractable.CompareTag("Door Button")) hoveredInteractable.GetComponent<DoorButton>().OpenDoor();
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
            //Interactable interactable = hit.collider.GetComponent<Interactable>();
            //if (interactable != null)
            //{
            //    GrabObject(interactable);
            //    interactable.OnPlayerWatching();
            //}

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
            }

            yield return null;
        }
    }

    IEnumerator EraseTextWithTimer(Text text, float time)
    {
        yield return new WaitForSeconds(time);
        text.text = "";
    }


}