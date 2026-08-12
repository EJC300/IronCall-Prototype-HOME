using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    [SerializeField] Transform target;
    
    void SetOffset()
    {
        Vector3 offsetDirection = target.position + offset;

        transform.position = offsetDirection;
    }
    
    private void LateUpdate()
    {
        SetOffset();
    }

}