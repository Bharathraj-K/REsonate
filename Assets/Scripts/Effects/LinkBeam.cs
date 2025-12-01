using UnityEngine;

// Visual beam component that connects player to linked enemies
public class LinkBeam : MonoBehaviour
{
    [Header("Beam Settings")]
    public float beamWidth = 0.1f;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1f);
    
    private Transform startPoint;
    private Transform endPoint;
    private LineRenderer lineRenderer;
    private Material beamMaterial;
    private float currentProgress = 0f;
    
    void Awake()
    {
        // Create LineRenderer component
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        // Configure LineRenderer
        lineRenderer.material = null; // Will be set in Initialize
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = 10; // Render on top
    }
    
    public void Initialize(Transform start, Transform end, Material material)
    {
        startPoint = start;
        endPoint = end;
        beamMaterial = material;
        
        if (lineRenderer != null && beamMaterial != null)
        {
            lineRenderer.material = beamMaterial;
        }
        
        UpdateBeamPosition();
    }
    
    void Update()
    {
        if (startPoint != null && endPoint != null)
        {
            UpdateBeamPosition();
        }
        else
        {
            // Destroy if either endpoint is missing
            Destroy(gameObject);
        }
    }
    
    void UpdateBeamPosition()
    {
        if (lineRenderer == null || startPoint == null || endPoint == null) return;
        
        // Update beam positions
        lineRenderer.SetPosition(0, startPoint.position);
        lineRenderer.SetPosition(1, endPoint.position);
    }
    
    public void UpdateProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);
        
        if (lineRenderer != null && beamMaterial != null)
        {
            // Update beam intensity based on progress
            float intensity = intensityCurve.Evaluate(currentProgress);
            
            // Create material instance to modify properties
            Material matInstance = lineRenderer.material;
            if (matInstance != null)
            {
                // Increase emission as link progresses
                Color emissionColor = matInstance.color * intensity * 2f;
                matInstance.SetColor("_EmissionColor", emissionColor);
                
                // Pulse effect
                float pulseSpeed = 5f + (currentProgress * 10f);
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.3f + 0.7f;
                lineRenderer.startWidth = beamWidth * pulse;
                lineRenderer.endWidth = beamWidth * pulse;
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up material instance if created
        if (lineRenderer != null && lineRenderer.material != null)
        {
            if (Application.isPlaying)
            {
                Destroy(lineRenderer.material);
            }
        }
    }
}