using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float hitHeight;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float jumpForce; // treat as force, not height
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private ThirdPersonCamera thirdPersonCamera; // drag same camera object here
    [SerializeField] private float rotationSpeed;
    public float SpeedMultiplier { get; set; } = 1f;
    public Rigidbody rb;
    private bool grounded;
    private bool jumpQueued;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
       
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            jumpQueued = true;
        }
    }

    void FixedUpdate()
    {
        CheckGrounded();
        Move();
        
        if (jumpQueued)
        {
            jumpQueued = false;
            Jump();
           
        }
       
    }

    void CheckGrounded()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        grounded = Physics.Raycast(ray, out RaycastHit hit, hitHeight, groundMask);

        if (grounded)
        {
            Vector3 gravityAlongSlope = Vector3.Cross( Vector3.ProjectOnPlane(Physics.gravity, hit.normal),transform.forward) - Physics.gravity;
            rb.AddForce(-gravityAlongSlope *10, ForceMode.Acceleration);
            Vector3 up = Vector3.Lerp(transform.up, hit.normal, Time.fixedDeltaTime * 500f);
            transform.up = up;
        }
        else
        {
            transform.up = Vector3.Lerp(transform.up, Vector3.up, Time.fixedDeltaTime * 50f);
        }
    }

    void Move()
    {
        float forwardInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, transform.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, transform.up).normalized;

        Vector3 inputDir = camForward * forwardInput + camRight * horizontalInput;
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        // In first person, the camera owns rotation directly (see ThirdPersonCamera.LateUpdate).
        // Only self-rotate toward movement when in third person, otherwise the two fight each other.
        bool cameraControlsRotation = thirdPersonCamera != null && thirdPersonCamera.IsFirstPerson;

        if (!cameraControlsRotation && inputDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir, transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }

        Vector3 targetVelocity = inputDir * maxSpeed * SpeedMultiplier;
        targetVelocity += Vector3.Project(rb.linearVelocity, transform.up);

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 10f);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }
}
