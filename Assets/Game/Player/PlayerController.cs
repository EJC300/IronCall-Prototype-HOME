using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpHeight;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundedHeight;
    [SerializeField] private float maxSpeed;
    private Vector3 movementInput;
    private Vector3 targetSpeed;
    private bool grounded = false;
    private bool canJump;
    private Rigidbody rb;
    void CheckGrounded()=> grounded = Physics.Raycast(new Ray(transform.position, -transform.up),out RaycastHit hit, groundedHeight,groundMask);
   
   

    void Jump()
    {
        if (canJump && grounded)
        {
            
            float forceForHeight = rb.mass * Mathf.Sqrt(2 * gravity * jumpHeight);
            Vector3 appliedForce = transform.up * forceForHeight;
            rb.AddForce(appliedForce,ForceMode.Impulse);
            canJump = false;
        }
    }
    void Move()
    {
        var movementSpeed = movementInput;
        movementSpeed = Vector3.ClampMagnitude(movementSpeed, 1f) * maxSpeed;
        targetSpeed = Vector3.Lerp(targetSpeed, movementSpeed, Time.deltaTime * 100);
        Quaternion forwardRotation = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0);
        transform.forward = forwardRotation * Vector3.forward;
        rb.linearVelocity = transform.TransformDirection( new Vector3(targetSpeed.x, rb.linearVelocity.y, targetSpeed.z));
       
    }
    void ControlPlayer()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            canJump = true;
        }
        movementInput.z = Input.GetAxis("Vertical");
        movementInput.x = Input.GetAxis("Horizontal");
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        ControlPlayer();
    }
    private void FixedUpdate()
    {
        CheckGrounded();
       
            Jump();
        if (grounded)
        {
            Move();
        }
    }


}
