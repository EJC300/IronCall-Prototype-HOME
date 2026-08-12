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
    private RaycastHit hit;
    private bool cantMove;
    void CheckGrounded()=> grounded = Physics.Raycast(new Ray(transform.position, -transform.up),out hit, groundedHeight,groundMask);


    void AlignWithGround()
    {
        if (grounded)
        {
            Vector3 groundDirection = (hit.point - transform.position).normalized;
            Vector3 groundUp = Vector3.Dot(groundDirection, -Vector3.up) * Vector3.up;
            transform.up = Vector3.MoveTowards(transform.up, groundUp, 1500 * Time.deltaTime);
        }
        else
        {
            transform.up = Vector3.up;
        }
        
    }
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
        if (canJump && movementInput.z > 0.0 && movementInput.x > 0.0)
        {
            cantMove = false;
        }
        else
        {
            cantMove = true;
        }
        
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        AlignWithGround();
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
    private void OnCollisionEnter(Collision collision)
    {
        //cantMove = !cantMove;
    }

}
