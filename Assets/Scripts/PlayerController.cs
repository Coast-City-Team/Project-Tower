using System;
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
    private float m_acceleration = 1f;
    [SerializeField]
    private float m_frictionValue = 0.3f;
    [SerializeField]
    private float m_maxPlayerSpeed = 0.1f;
    [SerializeField]
    private float m_maxPlayerSpeedDown = -1.0f;
    [Space]
    [Header("Movement Modifiers")]
    /*[SerializeField]
    [Range(0.1f, 1f)]
    private float m_strafePenalization = 0.5f;*/    
    [SerializeField]
    [Range(1f, 3f)]
    private float m_sprintingModifier = 1.25f;
    [SerializeField]
    [Range(0f, 1f)]
    private float m_airMovementPenalization = 0.5f;
    //[SerializeField]
    //private float m_smoothMovementSpeed = 0.2f;
    [Space]
    [Header("Jump Settings")]
    [SerializeField]
    private float m_jumpForce = 0.5f;
    [Space]
    [Header("Slide Settings")]

    private float floorY;

    // Movement interpolation variables
    private Vector2 currentInputVector;
    //private Vector2 smoothInputVelocity;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        inputManager = InputManager.Instance;
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;

        //ESTO ES TEMPORAL PUTO
        floorY = transform.position.y;
    }

    void Update()
    {
        m_isGrounded = transform.position.y == floorY;

        // Adjust player's rotation to camera
        transform.rotation = new Quaternion(transform.rotation.x, cameraTransform.rotation.y, transform.rotation.z, cameraTransform.rotation.w);

        if (m_isGrounded)
        {
            if (m_isSliding)
            {
                //Player is grounded and sliding
            }
            else
            {
                //Player is grounded and not sliding, apply friction
                if (m_velocity.magnitude >= MIN_VELOCITY_THRESHOLD)
                {
                    applyFriction();
                }
            }
            //Player is grounded
            checkJump();
        } else
        {
            //Player is flying, apply gravity
            applyGravity();
        }
        movePlayer();
    }

    private Vector3 getMovementVectorRelativeToCamera()
    {
        Vector2 playerMoveDir = inputManager.getPlayerMovement();
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

    // PRE CONDITION: m_isSliding is FALSE
    // Handles only horizontal movement
    private void movePlayer()
    {
        Vector3 currentInputVector = getMovementVectorRelativeToCamera();

        Vector3 deltaVelocity = currentInputVector * m_acceleration * Time.deltaTime;
        // TO DO : Arreglarlo para que modifique con la posicion relativa, no global
        //deltaVelocity.z *= m_strafePenalization;

        //Sprinting check
        float playerMaxVelocity = m_isSprinting && m_isGrounded ? m_maxPlayerSpeed * m_sprintingModifier : m_maxPlayerSpeed;

        //Add only horizontal movement
        Vector2 horizontalVelocity = new Vector2(m_velocity.x, m_velocity.z);
        Vector2 horizontalDeltaVelocity = new Vector2(deltaVelocity.x, deltaVelocity.z);

        //Reduce horizontal delta when jumping
        horizontalDeltaVelocity = m_isGrounded ? horizontalDeltaVelocity : horizontalDeltaVelocity * m_airMovementPenalization;

        //Limit horizontal speed
        horizontalVelocity = Vector2.ClampMagnitude(horizontalVelocity + horizontalDeltaVelocity, playerMaxVelocity);

        //Limit vertical speed 
        m_velocity.y = Mathf.Clamp(m_velocity.y, m_maxPlayerSpeedDown, 10000);

        m_velocity = new Vector3(horizontalVelocity.x, m_velocity.y, horizontalVelocity.y);

        transform.Translate(m_velocity, Space.World);
        
        
        //Falso piso!
        float targetY = transform.position.y + m_velocity.y;

        if (targetY <= floorY)
        {
            if(targetY < floorY)
            {
                Vector3 pos = transform.position;
                pos.y = floorY;
                transform.position = pos;
            }
            m_isGrounded = true;
            m_velocity.y = 0;
        }
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

    // PRE CONDITION: m_isGrounded is FALSE
    private void applyGravity()
    {
        m_velocity.y += -1.5f * Time.deltaTime;
    }

    // PRE CONDITION: m_isSliding is TRUE
    private void slidePlayer()
    {
        // TO DO
        m_isSliding = false;
    }

    // PRE CONDITION: m_isGrounded is TRUE
    private void checkJump()
    {
        if (inputManager.playerJumped())
        {
            jumpPlayer();
        }
    }

    private void checkSprint()
    {
        
    }

    private void jumpPlayer()
    {
        m_velocity.y += m_jumpForce;
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