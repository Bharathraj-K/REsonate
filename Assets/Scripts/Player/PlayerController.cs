using UnityEngine;

/// <summary>
/// Main player controller - handles movement, input, and boundary constraints
/// for the Resonance Core character in RESONATE!
/// </summary>
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private ResonanceCore resonanceCore;

    [Header("Configuration")]
    [Tooltip("Use centralized config, or override with local values if null")]
    public GameConfiguration gameConfig;
    
    [Header("Movement Settings (Local Override)")]
    public float forceValue = 50f;
    public float maxSpeed = 5f;
    
    [Header("Boundary Settings (Local Override)")]
    public float arenaRadius = 50f;
    public float boundaryForce = 100f;
    
    [Header("Camera Settings")]
    [Tooltip("Camera to use for movement direction (auto-finds if null)")]
    public Camera playerCamera;
    
    [Header("Input Settings")]
    [Tooltip("Alternative keys for color cycling (in addition to Q/E)")]
    public KeyCode[] colorCyclePrevious = { KeyCode.Q, KeyCode.LeftArrow };
    public KeyCode[] colorCycleNext = { KeyCode.E, KeyCode.RightArrow };
    
    [Tooltip("Direct color selection keys")]
    public KeyCode[] redColorKeys = { KeyCode.Alpha1, KeyCode.R };
    public KeyCode[] greenColorKeys = { KeyCode.Alpha2, KeyCode.G };
    public KeyCode[] blueColorKeys = { KeyCode.Alpha3, KeyCode.B };
    public KeyCode[] clearColorKeys = { KeyCode.Alpha0, KeyCode.Space, KeyCode.Escape };
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        resonanceCore = GetComponent<ResonanceCore>();
        
        // Auto-find camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindFirstObjectByType<Camera>();
            }
        }
    }
    
    void Update()
    {
        HandleColorInput();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
        HandleBoundaries();
    }
    
    void HandleColorInput()
    {
        // Mouse click color cycling
        if (Input.GetMouseButtonDown(0)) // Left click - cycle forward
        {
            resonanceCore.CycleColorForward();
        }
        
        if (Input.GetMouseButtonDown(1)) // Right click - cycle backward
        {
            resonanceCore.CycleColorBackward();
        }
        
        // Color cycling with configurable keys
        if (AnyKeyDown(colorCyclePrevious))
        {
            resonanceCore.CycleColorBackward();
        }
        
        if (AnyKeyDown(colorCycleNext))
        {
            resonanceCore.CycleColorForward();
        }
            
        // Direct color selection with configurable keys
        if (AnyKeyDown(redColorKeys))
        {
            resonanceCore.SetColor(ResonanceColor.Red);
        }
        
        if (AnyKeyDown(greenColorKeys))
        {
            resonanceCore.SetColor(ResonanceColor.Green);
        }
        
        if (AnyKeyDown(blueColorKeys))
        {
            resonanceCore.SetColor(ResonanceColor.Blue);
        }
        
        // Clear color with configurable keys
        if (AnyKeyDown(clearColorKeys))
        {
            resonanceCore.SetColor(ResonanceColor.None);
        }
    }
    
    /// <summary>
    /// Helper method to check if any key in an array was pressed this frame
    /// </summary>
    private bool AnyKeyDown(KeyCode[] keys)
    {
        foreach (KeyCode key in keys)
        {
            if (Input.GetKeyDown(key))
            {
                return true;
            }
        }
        return false;
    }
    
    void HandleMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Calculate movement relative to camera direction
        Vector3 movement = GetCameraRelativeMovement(moveHorizontal, moveVertical);
        
        // Get force value from config or local override
        float currentForce = gameConfig != null ? gameConfig.playerForce : forceValue;
        float currentMaxSpeed = gameConfig != null ? gameConfig.playerMaxSpeed : maxSpeed;
        
        // Apply movement force
        if (movement != Vector3.zero)
        {
            rb.AddForce(movement * currentForce, ForceMode.Force);
        }
        
        // Clamp velocity to max speed
        if (rb.linearVelocity.magnitude > currentMaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
        }
    }
    
    void HandleBoundaries()
    {
        // Get boundary values from config or local override
        float currentArenaRadius = gameConfig != null ? gameConfig.arenaRadius : arenaRadius;
        float currentBoundaryForce = gameConfig != null ? gameConfig.boundaryForce : boundaryForce;
        
        // Keep player within arena using repulsion force
        float distanceFromCenter = transform.position.magnitude;
        
        if (distanceFromCenter > currentArenaRadius)
        {
            // Calculate force direction toward center
            Vector3 forceDirection = -transform.position.normalized;
            
            // Apply stronger force the further outside the boundary
            float overshoot = distanceFromCenter - currentArenaRadius;
            float repulsionForce = currentBoundaryForce * overshoot;
            
            rb.AddForce(forceDirection * repulsionForce * Time.deltaTime, ForceMode.Force);
        }
    }
    
    /// <summary>
    /// Convert input to camera-relative movement direction
    /// </summary>
    Vector3 GetCameraRelativeMovement(float horizontal, float vertical)
    {
        if (playerCamera == null)
        {
            // Fallback to world-space movement if no camera
            return new Vector3(horizontal, 0f, vertical).normalized;
        }
        
        // Get camera's forward and right vectors (flattened to XZ plane)
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        
        // Remove Y component to keep movement on ground plane
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        
        // Normalize to ensure consistent movement speed
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // Calculate movement direction relative to camera
        Vector3 movement = (cameraForward * vertical) + (cameraRight * horizontal);
        
        return movement.normalized;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw arena boundary in editor (approximated with lines)
        Gizmos.color = Color.white;
        int segments = 32;
        float angleStep = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            Vector3 point1 = new Vector3(Mathf.Cos(angle1) * arenaRadius, 0, Mathf.Sin(angle1) * arenaRadius);
            Vector3 point2 = new Vector3(Mathf.Cos(angle2) * arenaRadius, 0, Mathf.Sin(angle2) * arenaRadius);
            Gizmos.DrawLine(point1, point2);
        }
        
        // Draw velocity vector for debugging
        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, rb.linearVelocity);
        }
    }
}