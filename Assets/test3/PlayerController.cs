using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public CapsuleCollider col;
    [HideInInspector] public Transform camTransform;

    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float acceleration = 50f;
    public float JMoveControl = 0.3f;
    public float maxAirSpeed = 10f;
    public float airAcceleration = 50f;
    public float groundCheckDistance = 0.2f;
    public float slopeLimit = 45f;
    public float jumpForce = 8f;
    public LayerMask groundMask;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool sprintHeld;

    [HideInInspector] public bool isGrounded;
    [HideInInspector] public Vector3 groundNormal;
    [HideInInspector] public float groundAngle;

    [HideInInspector] public PlayerStateMachine stateMachine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        camTransform = Camera.main.transform;

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");

        stateMachine = new PlayerStateMachine(this);
    }

    private void OnEnable()
    {
        jumpAction.started += OnJumpPerformed;
    }

    private void OnDisable()
    {
        jumpAction.started -= OnJumpPerformed;
    }

    private void Start()
    {
        stateMachine.Initialize(stateMachine.idleState);
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        sprintHeld = sprintAction.IsPressed();
        stateMachine.CurrentState.UpdateState();
    }

    private void FixedUpdate()
    {
        GroundCheck();
        stateMachine.CurrentState.FixedUpdateState();
    }

    private void GroundCheck()
    {
        Vector3 origin = transform.position + Vector3.up * (col.radius * 0.5f);
        float sphereRadius = col.radius * 0.9f;
        float maxDistance = groundCheckDistance + col.radius * 0.1f;

        if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit, maxDistance, groundMask))
        {
            isGrounded = true;
            groundNormal = hit.normal;
            groundAngle = Vector3.Angle(Vector3.up, groundNormal);
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            groundAngle = 0f;
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        jumpPressed = true;
    }

    public void ConsumeJump() => jumpPressed = false;

    public Vector3 GetMoveDirection()
    {
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        return (forward * moveInput.y + right * moveInput.x).normalized;
    }

#if false
    private void OnDrawGizmos()
    {
        col = GetComponent<CapsuleCollider>();
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + Vector3.up * (col.radius * 0.5f) + Vector3.down* (groundCheckDistance + col.radius * 0.1f), col.radius * 0.9f);
    }
#endif
}