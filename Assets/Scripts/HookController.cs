using UnityEngine;

public enum HookState
{
    Idle,
    Holding,
    Throwed,
    Attached,
    Released
}

public class HookController : MonoBehaviour
{
    private HookState m_hookState;

    private Rigidbody m_rb;
    private ConfigurableJoint m_jointWithPlayer;
    private Transform m_cameraTransform;
    private LineRenderer m_lineRenderer;
    private FixedJoint m_attachedToJoint;

    [Header("Hook Settings")]
    [SerializeField]
    private float m_hookLength = 15.0f;
    [SerializeField]
    private float m_hookIdleLength = 0.5f;
    [SerializeField]
    private Vector3 m_hookAnchor = new Vector3(-0.6f, 0.7f, 0f);
    [Space]

    [Header("Rope Settings")]
    [SerializeField]
    private float m_ropeDamper = 0f;
    [SerializeField]
    private float m_ropeSpringStiffness = 0f;
    [Space]

    [Header("Shoot Settings")]
    [SerializeField]
    private float m_hookShootForce = 10.0f;
    [SerializeField]
    private Transform m_hookShootPosition;
    [Space]

    [Header("Misc")]
    [SerializeField]
    private Rigidbody m_playerRigidbody;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_jointWithPlayer = GetComponent<ConfigurableJoint>();
        m_lineRenderer = GetComponent<LineRenderer>();
        m_cameraTransform = Camera.main.transform;
        m_hookState = HookState.Idle;
    }

    private void Update()
    {
        switch (m_hookState)
        {
            case HookState.Idle:
                m_lineRenderer.enabled = false;
                break;
            case HookState.Holding:
                transform.position = m_hookShootPosition.position;
                break;
            case HookState.Throwed:
                break;
            case HookState.Attached:
                break;
            case HookState.Released:
                break;
            default:
                break;
        }
    }

    private void LateUpdate()
    {
        if (m_hookState == HookState.Throwed || m_hookState == HookState.Attached || m_hookState == HookState.Released)
        {
            setLinePositions();
        }
    }

    public void HookButtonAction(bool buttonPressed)
    {
        if (buttonPressed)
        {
            switch (m_hookState)
            {
                case HookState.Idle:
                    ChargeHook();
                    break;
                case HookState.Attached:
                    ReleaseHook();
                    break;
                default:
                    break;
            }
        } else if (m_hookState == HookState.Holding)
        {
            ShootHook();
        }
        
    }

    private void setLinePositions()
    {
        m_lineRenderer.SetPosition(0, m_hookShootPosition.position);
        m_lineRenderer.SetPosition(1, transform.position);
    }

    private void ChargeHook()
    {
        m_hookState = HookState.Holding;
        Destroy(m_jointWithPlayer);
        m_jointWithPlayer = null;

        m_rb.useGravity = false;
    }

    private void ShootHook()
    {
        m_hookState = HookState.Throwed;
        m_rb.useGravity = true;

        setLinePositions();
        m_lineRenderer.enabled = true;

        m_rb.AddForce(m_hookShootForce * m_cameraTransform.forward, ForceMode.Impulse);

        RebuildJoint();
    }

    private void AttachHook(Rigidbody attachToRb)
    {
        m_attachedToJoint = gameObject.AddComponent<FixedJoint>();
        m_attachedToJoint.connectedBody = attachToRb;
    }

    private void ReleaseHook()
    {
        if (m_attachedToJoint != null)
        {
            Destroy(m_attachedToJoint);
        }
    }

    private void RebuildJoint()
    {
        m_jointWithPlayer = gameObject.AddComponent<ConfigurableJoint>();
        m_jointWithPlayer.axis = Vector3.forward;
        m_jointWithPlayer.connectedBody = m_playerRigidbody;
        m_jointWithPlayer.anchor = m_hookAnchor;

        m_jointWithPlayer.xMotion = m_jointWithPlayer.yMotion = m_jointWithPlayer.zMotion = ConfigurableJointMotion.Limited;
        m_jointWithPlayer.angularXMotion = m_jointWithPlayer.angularYMotion = m_jointWithPlayer.angularZMotion = ConfigurableJointMotion.Free;

        // Set length limit
        ChangeLengthJoint(m_hookLength);

        // Set spring settings to make it rope-like
        SoftJointLimitSpring jointLinearSpringLimit = m_jointWithPlayer.linearLimitSpring;
        jointLinearSpringLimit.damper = m_ropeDamper;
        jointLinearSpringLimit.spring = m_ropeSpringStiffness;
        m_jointWithPlayer.linearLimitSpring = jointLinearSpringLimit;
    }

    private void ChangeLengthJoint(float limitDistance)
    {
        SoftJointLimit jointLinearLimit = m_jointWithPlayer.linearLimit;
        jointLinearLimit.limit = limitDistance;
        m_jointWithPlayer.linearLimit = jointLinearLimit;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Hookable") && m_attachedToJoint == null)
        {
            m_hookState = HookState.Attached;
            AttachHook(collision.rigidbody);
        } else
        {
            m_hookState = HookState.Released;
        }
    }
}
