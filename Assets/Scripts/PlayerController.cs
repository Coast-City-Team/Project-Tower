using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private InputManager inputManager;
    private Transform cameraTransform;

    private Vector3 playerVelocity;
    private bool m_groundedPlayer;
    [SerializeField]
    private float m_playerSpeed = 2.0f;
    [SerializeField]
    private float m_jumpHeight = 1.0f;
    private float gravityValue = -9.81f;

    private void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
        controller.minMoveDistance = 0;
        inputManager = InputManager.Instance;
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        m_groundedPlayer = controller.isGrounded;
        if (m_groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 playerMovement = inputManager.GetPlayerMovement();
        Vector3 move = new Vector3(playerMovement.x, 0f, playerMovement.y);
        move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
        move.y = 0f;
        controller.Move(move * Time.deltaTime * m_playerSpeed);

        // Changes the height position of the player..
        if (inputManager.PlayerJumped() && m_groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(m_jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
