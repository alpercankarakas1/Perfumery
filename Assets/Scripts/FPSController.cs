using UnityEngine;
using UnityEngine.InputSystem; // Input System için gerekli

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;
    
    CharacterController characterController;

    // Input System için değişkenler
    private PlayerInputActions playerInputActions;
    private Vector2 currentMovementInput;
    private Vector2 currentLookInput;
    private bool isRunningInput;
    private bool jumpInput;

    void Awake()
    {
        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.Move.performed += ctx => currentMovementInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += ctx => currentMovementInput = Vector2.zero;

        playerInputActions.Player.Look.performed += ctx => currentLookInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Look.canceled += ctx => currentLookInput = Vector2.zero;

        playerInputActions.Player.Jump.performed += ctx => jumpInput = true;
        playerInputActions.Player.Jump.canceled += ctx => jumpInput = false; 

        playerInputActions.Player.Sprint.performed += ctx => isRunningInput = true;
        playerInputActions.Player.Sprint.canceled += ctx => isRunningInput = false;
    }

    void OnEnable()
    {
        playerInputActions.Enable();
    }

    void OnDisable()
    {
        playerInputActions.Disable();
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        #region Handles Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        
        float currentSpeed = isRunningInput ? runSpeed : walkSpeed;
        float curSpeedX = canMove ? currentSpeed * currentMovementInput.y : 0; // currentMovementInput.y dikey hareketi (W/S) verir
        float curSpeedY = canMove ? currentSpeed * currentMovementInput.x : 0; // currentMovementInput.x yatay hareketi (A/D) verir
        
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        #endregion

        #region Handles Jumping
        if (jumpInput && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        #endregion

        #region Handles Rotation
        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -currentLookInput.y * lookSpeed; // Fare Y ekseni yukarı/aşağı bakış
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, currentLookInput.x * lookSpeed, 0); // Fare X ekseni sola/sağa dönüş
        }
        #endregion
    }
}