using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Transform cameraTransform;
    private InputManager inputManager;

    [SerializeField]
    private bool m_groundedPlayer;
    [SerializeField]
    private float m_accelerationForce = 50.0f;
    [SerializeField]
    private float m_maxPlayerSpeed = 10.0f;
    [SerializeField]
    private float m_jumpForce = 500.0f;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        inputManager = InputManager.Instance;
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        if (m_groundedPlayer)
        {
            rb.velocity.Set(rb.velocity.x, 0f, rb.velocity.z);
        }

        if (m_groundedPlayer)
        {
            MovePlayer();
        }
    }

    void Update()
    {
        m_groundedPlayer = rb.velocity.y > -0.01f && rb.velocity.y < 0.01f;
    }

    private void MovePlayer()
    {
        Vector2 playerMoveDir = inputManager.GetPlayerMovement();
        Vector3 moveDirection = new Vector3(playerMoveDir.x, 0f, playerMoveDir.y);
        moveDirection = Vector3.Normalize(transform.forward * moveDirection.z + transform.right * moveDirection.x);

        transform.rotation = new Quaternion(transform.rotation.x, cameraTransform.rotation.y, transform.rotation.z, transform.rotation.w);

        rb.AddForce(moveDirection * m_accelerationForce * Time.fixedDeltaTime, ForceMode.VelocityChange);

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, m_maxPlayerSpeed);
    }

    public void OnJump()
    {
        rb.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse); 
    }
}