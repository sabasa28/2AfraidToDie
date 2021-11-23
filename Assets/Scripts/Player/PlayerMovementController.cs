using Photon.Pun;
using UnityEngine;

public class PlayerMovementController : MonoBehaviourPun
{
    [Header("Camera")]
    [SerializeField] float cameraSpeed = 0.0f;
    [SerializeField] Vector3 cameraInitialPosition = Vector3.zero;
    [SerializeField] Vector3 cameraInitialRotation = Vector3.zero;
    [HideInInspector] public Transform cameraTransform = null;
    [SerializeField] Transform cameraHolder = null;

    float cameraRotationX;

    [Header("Player Controller")]
    [SerializeField] float movementSpeed = 0.0f;
    [HideInInspector] public bool ableToMove = true;
    CharacterController characterController = null;
    Rigidbody rigidBody = null;

    [Header("Physics")]
    [SerializeField] Transform groundCheck = null;
    [SerializeField] LayerMask walkableLayer = 0;
    [SerializeField] float gravityForce = 0.0f;
    [SerializeField] float groundCheckRadius = 0.0f;

    bool canProcessInput = true;
    bool rotationActive = true;
    bool isGrounded;
    Vector3 velocity;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        rigidBody = GetComponent<Rigidbody>();

        rigidBody.useGravity = photonView.IsMine;
    }

    void OnEnable() => UIManager_Gameplay.OnPauseMenuStateSwitched += OnPauseMenuStateSwitched;

    void Start()
    {
        if (photonView.IsMine)
        {
            cameraTransform.SetParent(cameraHolder);
            cameraTransform.localPosition = cameraInitialPosition;
            cameraTransform.localRotation = Quaternion.Euler(cameraInitialRotation);
        }

        characterController.enabled = true;

        SetCursorLockState(true);

        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (canProcessInput)
        {
        #region Mouse Input
        if (rotationActive)
        {
            Vector3 viewRotation;
            viewRotation = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")) * cameraSpeed;

            characterController.transform.Rotate(new Vector3(0, 1, 0), viewRotation.y);
            cameraRotationX += viewRotation.x;
            cameraRotationX = Mathf.Clamp(cameraRotationX, -90.0f, 90.0f);
            cameraTransform.localRotation = Quaternion.Euler(cameraRotationX, cameraTransform.localRotation.eulerAngles.y, 0);
        }
        #endregion

        #region Keyboard Input
        float inputX = -Input.GetAxis("Vertical");
        float inputZ = Input.GetAxis("Horizontal");
        Vector3 movement;
        movement = (inputX * transform.right + inputZ * transform.forward) * movementSpeed;
        if (ableToMove && characterController.enabled) characterController.Move(movement * Time.deltaTime);
        #endregion
        }

        #region Physics
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, walkableLayer);

        if (isGrounded) velocity = Vector3.zero;
        else velocity.y += gravityForce * Time.deltaTime;
        if (ableToMove && characterController.enabled) characterController.Move(velocity * Time.deltaTime);
        #endregion
    }

    void OnDisable() => UIManager_Gameplay.OnPauseMenuStateSwitched -= OnPauseMenuStateSwitched;

    void OnPauseMenuStateSwitched(bool state)
    {
        canProcessInput = !state;
        SetCursorLockState(!state);
    }

    public void SetCursorLockState(bool isActive)
    {
        Cursor.lockState = isActive ? CursorLockMode.Locked : CursorLockMode.Confined;
        Cursor.visible = !isActive;
    }

    public void LockCameraLookingUp(bool locked)
    {
        rotationActive = !locked;
        if (locked)
        {
            cameraTransform.localRotation = Quaternion.Euler(-90.0f, 0.0f, -90.0f);
        }
        else
        {
            cameraTransform.localRotation = Quaternion.Euler(cameraInitialRotation);
        }
    }

    public void SetRotationActiveState(bool isActive) => rotationActive = isActive;

    public void SetCharacterControllerActiveState(bool isActive) => characterController.enabled = isActive;
}