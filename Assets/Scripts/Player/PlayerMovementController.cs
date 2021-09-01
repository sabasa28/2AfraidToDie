using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] Transform cameraTransform = null;
    [SerializeField] float cameraSpeed = 0.0f;

    float cameraRotationX;

    [Header("Player Controller")]
    [SerializeField] float movementSpeed = 0.0f;
    [HideInInspector] public bool ableToMove = true;
    CharacterController characterController = null;

    [Header("Physics")]
    [SerializeField] Transform groundCheck = null;
    [SerializeField] LayerMask walkableLayer = 0;
    [SerializeField] float gravityForce = 0.0f;
    [SerializeField] float groundCheckRadius = 0.0f;

    bool isGrounded;
    Vector3 velocity;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        characterController.enabled = false;
    }

    void Start()
    {
        characterController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        #region Mouse Input
        Vector3 viewRotation;
        viewRotation = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")) * cameraSpeed;

        characterController.transform.Rotate(new Vector3(0, 1, 0), viewRotation.y * Time.deltaTime);
        cameraRotationX += viewRotation.x * Time.deltaTime;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90.0f, 90.0f);
        cameraTransform.localRotation = Quaternion.Euler(cameraRotationX, cameraTransform.localRotation.eulerAngles.y, 0);
        #endregion

        #region Keyboard Input
        float inputX = -Input.GetAxis("Vertical");
        float inputZ = Input.GetAxis("Horizontal");
        Vector3 movement;
        movement = (inputX * transform.right + inputZ * transform.forward) * movementSpeed;
        if (ableToMove && characterController.enabled) characterController.Move(movement * Time.deltaTime);
        #endregion

        #region Physics
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, walkableLayer);

        if (isGrounded) velocity = Vector3.zero;
        else velocity.y += gravityForce * Time.deltaTime;
        if (ableToMove && characterController.enabled) characterController.Move(velocity * Time.deltaTime);
        #endregion
    }

    public void setCharacterControllerActiveState(bool isActive)
    {
        characterController.enabled = isActive;
    }
}