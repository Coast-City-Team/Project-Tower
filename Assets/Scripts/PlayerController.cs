using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private const float MIN_VELOCITY_THRESHOLD = 0.001f;

    private Rigidbody rb;
    private Transform cameraTransform;
    private InputManager inputManager;
    private HookController hookController;

    private bool m_isGrounded = true;
    private bool m_isSprinting = false;
    private bool m_isSliding = false;
    private Vector3 m_velocity = Vector3.zero;

    [Header("Movement Settings")]
    [SerializeField]
    private float m_acceleration = 3f;
    [SerializeField]
    private float m_frictionValue = 0.5f;
    [SerializeField]
    private float m_maxPlayerSpeed = 6f;
    [SerializeField]
    private float m_maxVelocityY = 5.0f;
    [Space]
    [Header("Movement Modifiers")]  
    [SerializeField]
    [Range(1f, 3f)]
    private float m_sprintingModifier = 1.5f;
    [SerializeField]
    [Range(0f, 1f)]
    private float m_airMovementPenalization = 0.2f;
    [Space]
    [Header("Jump Settings")]
    [SerializeField]
    private float m_jumpVelocity = 0.5f;

    private float floorY = 1f;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        inputManager = InputManager.Instance;
        cameraTransform = Camera.main.transform;
        hookController = GetComponentInChildren<HookController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        // Adjust player's rotation to camera
        Quaternion newRotation = new Quaternion(transform.rotation.x, cameraTransform.rotation.y, transform.rotation.z, cameraTransform.rotation.w);
        rb.MoveRotation(newRotation.normalized);

        if (m_isGrounded)
        {
            // Player is grounded and moving, apply friction
            if (m_velocity.magnitude >= MIN_VELOCITY_THRESHOLD)
            {
                ApplyFriction();
            }
        } else
        {
            // If player is in the air, apply gravity
            ApplyGravity();
        }

        MovePlayer();
    }

    void Update()
    {
        m_isGrounded = Mathf.Approximately(transform.position.y, floorY);
    }

    private Vector3 GetMovementVectorRelativeToCamera()
    {
        Vector2 playerMoveDir = inputManager.GetPlayerMovement();

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 forwardRelativeInput = playerMoveDir.y * cameraForward;
        Vector3 rightRelativeInput = playerMoveDir.x * cameraRight;

        return forwardRelativeInput + rightRelativeInput;
    }

    // Handles xz movement
    private void MovePlayer()
    {
        Vector3 currentInputVector = GetMovementVectorRelativeToCamera();
        Vector3 deltaVelocity = m_acceleration * Time.fixedDeltaTime * currentInputVector;
        // TO DO : Add speed penalization when moving to the sides

        // Sprinting check
        float playerMaxVelocity = m_isSprinting && m_isGrounded ? m_maxPlayerSpeed * m_sprintingModifier : m_maxPlayerSpeed;
        playerMaxVelocity *= Time.fixedDeltaTime;

        // Change only xz movement
        Vector2 xzVelocity = new Vector2(m_velocity.x, m_velocity.z);
        Vector2 xzDeltaVelocity = new Vector2(deltaVelocity.x, deltaVelocity.z);

        // Reduce xz delta when player is not grounded
        xzDeltaVelocity = m_isGrounded ? xzDeltaVelocity : xzDeltaVelocity * m_airMovementPenalization;

        // Limit xz speed
        xzVelocity = Vector2.ClampMagnitude(xzVelocity + xzDeltaVelocity, playerMaxVelocity);

        // Limit vertical speed
        float playerMaxVelocityY = m_maxVelocityY * Time.fixedDeltaTime;

        m_velocity = new Vector3(xzVelocity.x, Mathf.Clamp(m_velocity.y, -playerMaxVelocityY, Mathf.Infinity), xzVelocity.y);
        rb.MovePosition(transform.position + m_velocity);

        //Falso piso!
        /*float targetY = transform.position.y + m_velocity.y;

        if (targetY <= floorY)
        {
            if (targetY < floorY)
            {
                Vector3 pos = transform.position;
                pos.y = floorY;
                transform.position = pos;
            }
            m_velocity.y = 0;
        }*/
 
    }

    // PRE CONDITION: m_isGrounded is TRUE
    private void ApplyFriction()
    {
        Vector3 frictionVector = -m_velocity.normalized * m_frictionValue * Time.fixedDeltaTime;
        m_velocity += frictionVector;

        if (Mathf.Abs(m_velocity.x) < MIN_VELOCITY_THRESHOLD)
        {
            m_velocity.x = 0f;
        }
        if (Mathf.Abs(m_velocity.z) < MIN_VELOCITY_THRESHOLD)
        {
            m_velocity.z = 0f;
        }
    }

    // PRE CONDITION: m_isGrounded is FALSE
    private void ApplyGravity()
    {
        m_velocity.y += Physics.gravity.y * Time.fixedDeltaTime;
    }

    // PRE CONDITION: m_isSliding is TRUE
    private void SlidePlayer()
    {
        // TO DO
        m_isSliding = false;
    }

    // PRE CONDITION: m_isGrounded is TRUE
    private void JumpPlayer()
    {
        m_velocity.y += m_jumpVelocity * Time.fixedDeltaTime;
    }

    public void OnSprint()
    {
        m_isSprinting = !m_isSprinting;
    }

    public void OnSlide()
    {
        if (m_isGrounded && !m_isSliding && rb.velocity.magnitude > MIN_VELOCITY_THRESHOLD)
        {
            m_isSliding = true;
        }
    }

    public void OnJump()
    {
        if (m_isGrounded)
        {
            JumpPlayer();
        }
    }

    public void OnHook(InputValue inputValue)
    {
        hookController.HookButtonAction(inputValue.isPressed);
    }
}