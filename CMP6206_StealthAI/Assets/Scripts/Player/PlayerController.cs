using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float lookXLimit = 80.0f;

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private Vector3 standingCenter = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 crouchCenter = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private float cameraStandingY = 0.6f;
    [SerializeField] private float cameraCrouchY = 0.0f; // Equivale a 1 m de altura respecto al suelo

    [Header("Stealth & Noise System")]
    public float currentNoiseRadius = 0f;
    [SerializeField] private float walkNoiseRadius = 8.0f;
    [SerializeField] private float crouchNoiseRadius = 1.5f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private bool isCrouching = false;

    public bool IsCrouching => isCrouching;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        // Asegurar configuración base del CharacterController
        characterController.height = standingHeight;
        characterController.center = standingCenter;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform != null)
        {
            Vector3 camPos = cameraTransform.localPosition;
            camPos.y = cameraStandingY;
            cameraTransform.localPosition = camPos;
        }
    }

    private void Update()
    {
        HandleMouseLook();
        HandleCrouch();
        HandleMovement();
    }

    private void HandleMovement()
    {
        float currentSpeed = isCrouching ? crouchSpeed : walkSpeed;

        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector3 move = (forward * moveVertical + right * moveHorizontal).normalized;

        if (characterController.isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f;
        }

        moveDirection.x = move.x * currentSpeed;
        moveDirection.z = move.z * currentSpeed;
        moveDirection.y += gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);

        // Radio sonoro para los sensores acústicos de la IA
        bool isMoving = move.sqrMagnitude > 0.01f;
        if (!isMoving)
        {
            currentNoiseRadius = 0f;
        }
        else
        {
            currentNoiseRadius = isCrouching ? crouchNoiseRadius : walkNoiseRadius;
        }
    }

    private void HandleMouseLook()
    {
        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.rotation *= Quaternion.Euler(0f, mouseX, 0f);
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            SetCrouchState(!isCrouching);
        }
    }

    private void SetCrouchState(bool crouch)
    {
        isCrouching = crouch;
        
        // Mantener la base del cilindro fija en el suelo:
        characterController.height = isCrouching ? crouchHeight : standingHeight;
        characterController.center = isCrouching ? crouchCenter : standingCenter;

        if (cameraTransform != null)
        {
            Vector3 camPos = cameraTransform.localPosition;
            camPos.y = isCrouching ? cameraCrouchY : cameraStandingY;
            cameraTransform.localPosition = camPos;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currentNoiseRadius);
    }
}