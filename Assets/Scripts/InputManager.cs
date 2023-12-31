using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerControls playerControls;
    private static InputManager _instance;

    public static InputManager Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else
        {
            _instance = this;
        }
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public Vector2 GetPlayerMovement()
    {
        return playerControls.Player1.Movement.ReadValue<Vector2>();
    }

    public bool PlayerJumped()
    {
        return playerControls.Player1.Jump.triggered;
    }

    public Vector2 getPlayerMouseDelta()
    {
        return playerControls.Player1.Look.ReadValue<Vector2>();
    }
}
