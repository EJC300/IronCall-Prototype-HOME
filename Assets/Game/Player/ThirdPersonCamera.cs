using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    [SerializeField] Transform target;
    [SerializeField] private float maxAngle, minAngle;
    private Vector3 angle;
    private Quaternion orbitRotation;
    private Vector3 currentOffset;
    private Vector3 pivot;

   public bool isFirstPerson;
    void SetOffset()
    {
        Vector3 offsetDirection =  target.position + orbitRotation* currentOffset;

        transform.position = offsetDirection;
    }
    void ChangePerspective()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            this.isFirstPerson = !this.isFirstPerson;
         
        }
        
        Vector3 firstPerson = new Vector3(0,0.9f,0);

     
  
            if (isFirstPerson)
            {
                
                currentOffset = firstPerson;
            }
            else if(!isFirstPerson) 
            {
                
                currentOffset = offset;
            }
           
        
        
    }
    void Orbit()
    {
        angle.y += Input.GetAxis("Mouse X") * 10;
        angle.x += Input.GetAxis("Mouse Y") * 10;
        angle.x = Mathf.Clamp(angle.x, -minAngle, maxAngle);
        orbitRotation = Quaternion.Euler(angle);
        
        transform.position = target.position + new Vector3(0,1.19f,3) - orbitRotation  * Vector3.forward;
        transform.rotation = Quaternion.Slerp(transform.rotation, orbitRotation,Time.fixedDeltaTime * 100);
    }
    private void Update()
    {
        ChangePerspective();
    }
    private void LateUpdate()
    {
        
        SetOffset();
        
    }
    private void FixedUpdate()
    {
        Orbit();
    }
}