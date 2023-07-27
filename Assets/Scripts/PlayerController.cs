using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private InputManager inputManager;
    private Transform cameraTransform;

    private bool m_groundedPlayer;
    [SerializeField]
    private float m_accelerationForce = 20.0f;
    [SerializeField]
    private float m_maxPlayerSpeed = 10.0f;
    [SerializeField]
    private float m_jumpHeight = 3.0f;

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

        if (inputManager.PlayerJumped() && m_groundedPlayer)
        {
            Jump();
        }
        MovePlayer();
    }

    void Update()
    {
        m_groundedPlayer = Mathf.Approximately(rb.velocity.y, 0);

    }

    private void MovePlayer()
    {
        Vector2 playerMovement = inputManager.GetPlayerMovement();
        Vector3 moveDirection = new Vector3(playerMovement.x, 0f, playerMovement.y);
        moveDirection = Vector3.Normalize(cameraTransform.forward * moveDirection.z + cameraTransform.right * moveDirection.x);

        rb.AddForce(moveDirection * m_accelerationForce * Time.fixedDeltaTime, ForceMode.VelocityChange);

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, m_maxPlayerSpeed);
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * m_jumpHeight, ForceMode.Impulse);
    }
}