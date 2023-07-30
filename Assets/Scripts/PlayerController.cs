using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const float MIN_VELOCITY_THRESHOLD = 0.001f;
    private Rigidbody rb;
    private Transform cameraTransform;
    private InputManager inputManager;

    private bool m_isGrounded;
    private bool m_isSprinting = false;
    private bool m_isSliding = false;

    [Header("Movement Settings")]
    [SerializeField]
    private float m_acceleration = 50.0f;
    [SerializeField]
    private float m_maxPlayerSpeed = 10.0f;
    [Space]
    [Header("Movement Modifiers")]
    [SerializeField]
    [Range(0.1f, 1f)]
    private float m_strafePenalization = 0.5f;
    [SerializeField]
    [Range(1f, 3f)]
    private float m_sprintingModifier = 1.25f;
    [SerializeField]
    private float m_smoothMovementSpeed = 0.2f;
    [Space]
    [Header("Jump Settings")]
    [SerializeField]
    private float m_jumpForce = 500.0f;
    [Space]
    [Header("Slide Settings")]

    // Movement interpolation variables
    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        inputManager = InputManager.Instance;
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        if (m_isGrounded)
        {
            rb.velocity.Set(rb.velocity.x, 0f, rb.velocity.z);
        }

        if (m_isGrounded && !m_isSliding)
        {
            MovePlayer();
        }

        if (m_isSliding)
        {
            SlidePlayer();
        }
    }

    void Update()
    {
        m_isGrounded = rb.velocity.y > -0.01f && rb.velocity.y < 0.01f;

        // Adjust player's rotation to camera
        transform.rotation = new Quaternion(transform.rotation.x, cameraTransform.rotation.y, transform.rotation.z, transform.rotation.w);
    }

    private Vector3 GetMovementVectorRelativeToCamera()
    {
        Vector2 playerMoveDir = inputManager.GetPlayerMovement();
        currentInputVector = Vector2.SmoothDamp(currentInputVector, playerMoveDir, ref smoothInputVelocity, m_smoothMovementSpeed);
        
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 forwardRelativeInput = currentInputVector.y * cameraForward;
        Vector3 rightRelativeInput = currentInputVector.x * cameraRight;

        return forwardRelativeInput + rightRelativeInput;
    }

    // PRE CONDITION: m_isGrounded is TRUE AND m_isSliding is FALSE
    private void MovePlayer()
    {
        Vector3 currentInputVector = GetMovementVectorRelativeToCamera();

        Vector3 deltaVelocity = currentInputVector * m_acceleration * Time.fixedDeltaTime;
        deltaVelocity.z *= m_strafePenalization;

        float playerMaxVelocity = m_isSprinting ? m_maxPlayerSpeed * m_sprintingModifier : m_maxPlayerSpeed;
        rb.velocity = Vector3.ClampMagnitude(deltaVelocity + rb.velocity, playerMaxVelocity);
    }

    // PRE CONDITION: m_isSliding is TRUE
    private void SlidePlayer()
    {
        // TO DO
        m_isSliding = false;
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
            rb.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
        }
    }
}