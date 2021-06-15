using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("GeneralReferences")]
    [SerializeField] Transform cameraTransform = null;
    [SerializeField] Transform groundCheck = null;
    CharacterController cc;
    
    [Header("Movement")] 
    [SerializeField] float movementSpeed = 0.0f;
    [SerializeField] float cameraSpeed = 0.0f;
    float camRotX;
    float lastCameraDirX;
    float lastCameraDirY;

    [Space]
    [Header("Physics")]
    [SerializeField] float gravityForce = 0.0f;
    [SerializeField] bool isGrounded;
    [SerializeField] float groundCheckRadius = 0.0f;
    [SerializeField] LayerMask walkableLayer = 0;
    Vector3 velocity;

    Material mat;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cc = GetComponent<CharacterController>();
        mat = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
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
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 10.0f, 1 << LayerMask.NameToLayer("Interactable"));

        if (hit.collider != null)
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null) GrabObject(interactable);
        }
        Debug.DrawRay(cameraTransform.position,cameraTransform.forward*10.0f,Color.red);
        
        
    }

    private void GrabObject(Interactable interactable)
    {
        interactable.OnPlayerWatching();
        interactable.transform.parent = cameraTransform;
    }
}
