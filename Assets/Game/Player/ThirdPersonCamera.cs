using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 headOffset = new Vector3(0f, 1.6f, 0f); // eye/pivot height on target

    [Header("Orbit Settings")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float mouseSensitivityX = 3f;
    [SerializeField] private float mouseSensitivityY = 3f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;
    [SerializeField] private float rotationSmoothTime = 0.05f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionRadius = 0.3f;

    [Header("First Person")]
    [SerializeField] private KeyCode toggleKey = KeyCode.C;
    [SerializeField] private bool startInFirstPerson = false;

    [Header("Vignette (Prototype)")]
    [SerializeField] private Shader vignetteShader;
    [Range(0f, 1f)] public float vignetteIntensity = 0f; // wire your input to this later
    [SerializeField] private Color vignetteColor = Color.black;
    [Range(0.1f, 3f)][SerializeField] private float vignetteSmoothness = 1.2f;
    [Range(0f, 2f)][SerializeField] private float vignetteRoundness = 1f;
    private Material vignetteMaterial;

    private float targetYaw;
    private float targetPitch;
    private float currentYaw;
    private float currentPitch;
    private float yawVelocity;
    private float pitchVelocity;
    private bool isFirstPerson;
    public bool IsFirstPerson => isFirstPerson;
    private void Awake()
    {
        isFirstPerson = startInFirstPerson;

        if (vignetteShader == null)
            vignetteShader = Shader.Find("Hidden/PrototypeVignette");

        if (vignetteShader != null)
            vignetteMaterial = new Material(vignetteShader);

        Vector3 angles = transform.eulerAngles;
        targetYaw = currentYaw = angles.y;
        targetPitch = currentPitch = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            isFirstPerson = !isFirstPerson;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        targetYaw += mouseX * mouseSensitivityX;
        targetPitch -= mouseY * mouseSensitivityY;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scroll * 5f, minDistance, maxDistance);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, rotationSmoothTime);
        currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, rotationSmoothTime);

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 pivot = target.position + headOffset;

        if (isFirstPerson)
        {
            transform.position = pivot;
            transform.rotation = rotation;
        }
        else
        {
            Vector3 desiredPosition = pivot - (rotation * Vector3.forward * distance);

            if (Physics.SphereCast(pivot, collisionRadius, (desiredPosition - pivot).normalized, out RaycastHit hit, distance, collisionMask))
            {
                desiredPosition = pivot - (rotation * Vector3.forward * hit.distance);
            }

            transform.position = desiredPosition;
            transform.rotation = rotation;
        }
        
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (vignetteMaterial == null || vignetteIntensity <= 0f)
        {
            Graphics.Blit(source, destination);
            return;
        }

        vignetteMaterial.SetColor("_VignetteColor", vignetteColor);
        vignetteMaterial.SetFloat("_Intensity", vignetteIntensity);
        vignetteMaterial.SetFloat("_Smoothness", vignetteSmoothness);
        vignetteMaterial.SetFloat("_Roundness", vignetteRoundness);

        Graphics.Blit(source, destination, vignetteMaterial);
    }
}