using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const float MIN_VELOCITY_THRESHOLD = 0.001f;
    private Rigidbody rb;
    private Transform cameraTransform;
    private InputManager inputManager;

    [SerializeField]
    private bool m_isGrounded;
    private bool m_isSprinting = false;
    private bool m_isSliding = false;
    [SerializeField]
    private Vector3 m_velocity = Vector3.zero;

    [Header("Movement Settings")]
    [SerializeField]
    private float m_acceleration = 3.0f;
    [SerializeField]
    private float m_frictionValue = 0.5f;
    [SerializeField]
    private float m_maxPlayerSpeed = 10.0f;
    [Space]
    [Header("Movement Modifiers")]
    /*[SerializeField]
    [Range(0.1f, 1f)]
    private float m_strafePenalization = 0.5f;*/
    [SerializeField]
    [Range(1f, 3f)]
    private float m_sprintingModifier = 1.25f;
    //[SerializeField]
    //private float m_smoothMovementSpeed = 0.2f;
    [Space]
    [Header("Jump Settings")]
    [SerializeField]
    private float m_jumpForce = 300.0f;
    [Space]
    [Header("Slide Settings")]

    // Movement interpolation variables
    private Vector2 currentInputVector;
    //private Vector2 smoothInputVelocity;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        inputManager = InputManager.Instance;
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        m_isGrounded = Mathf.Abs(m_velocity.y) < MIN_VELOCITY_THRESHOLD;

        if (m_isGrounded)
        {
            m_velocity.y = 0f;
        }

        // Adjust player's rotation to camera
        transform.rotation = new Quaternion(transform.rotation.x, cameraTransform.rotation.y, transform.rotation.z, cameraTransform.rotation.w);

        if (m_isGrounded && !m_isSliding)
        {
            MovePlayer();
            if (m_velocity.magnitude >= MIN_VELOCITY_THRESHOLD)
            {
                applyFriction();
            }

        }
    }

    private Vector3 GetMovementVectorRelativeToCamera()
    {
        Vector2 playerMoveDir = inputManager.GetPlayerMovement();
        //currentInputVector = Vector2.SmoothDamp(currentInputVector, playerMoveDir, ref smoothInputVelocity, m_smoothMovementSpeed);
        currentInputVector = playerMoveDir;

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

        Vector3 deltaVelocity = currentInputVector * m_acceleration * Time.deltaTime;
        // TO DO : Arreglarlo para que modifique con la posicion relativa, no global
        //deltaVelocity.z *= m_strafePenalization;

        float playerMaxVelocity = m_isSprinting ? m_maxPlayerSpeed * m_sprintingModifier : m_maxPlayerSpeed;
        m_velocity = Vector3.ClampMagnitude(deltaVelocity + m_velocity, playerMaxVelocity);
        transform.Translate(m_velocity, Space.World);
    }

    // PRE CONDITION: m_isGrounded is TRUE
    private void applyFriction()
    {
        Vector3 frictionVector = -m_velocity.normalized * m_frictionValue * Time.deltaTime;
        m_velocity += frictionVector;

        // If velocity has changed of sign, it is set to zero
        if (Mathf.Abs(m_velocity.x) < MIN_VELOCITY_THRESHOLD)
        {
            m_velocity.x = 0f;
        }
        if (Mathf.Abs(m_velocity.z) < MIN_VELOCITY_THRESHOLD)
        {
            m_velocity.z = 0f;
        }
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