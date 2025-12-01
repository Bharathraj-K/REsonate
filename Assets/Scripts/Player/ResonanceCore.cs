using UnityEngine;

/// <summary>
/// Resonance Core behavior - handles color switching, visual feedback, 
/// and pulse effects for the player character
/// </summary>
public class ResonanceCore : MonoBehaviour
{
    [Header("Visual Settings")]
    public Material whiteFaceMaterial;
    public Material redFaceMaterial;
    public Material greenFaceMaterial;
    public Material blueFaceMaterial;
    
    [Header("Animation Settings")]
    public float colorTransitionSpeed = 5f;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.3f;
    
    private ResonanceColor currentColor = ResonanceColor.None;
    private ResonanceColor targetColor = ResonanceColor.None;
    private Renderer sphereRenderer;
    private Material currentFaceMaterial;
    private bool isTransitioning = false;
    
    // Color cycling array for Q/E keys
    private ResonanceColor[] colorCycle = { 
        ResonanceColor.Red, 
        ResonanceColor.Green, 
        ResonanceColor.Blue 
    };
    private int currentColorIndex = 0;
    
    void Start()
    {
        sphereRenderer = GetComponent<Renderer>();
        SetColor(ResonanceColor.None);
    }
    
    void Update()
    {
        // Handle color transitions and pulse effects
        if (isTransitioning)
        {
            HandleColorTransition();
        }
        
        // Add pulse effect to active colors
        if (currentColor != ResonanceColor.None)
        {
            HandlePulseEffect();
        }
    }
    
    public void SetColor(ResonanceColor newColor)
    {
        if (targetColor != newColor)
        {
            targetColor = newColor;
            isTransitioning = true;
            
            // Update color index to match the new color
            for (int i = 0; i < colorCycle.Length; i++)
            {
                if (colorCycle[i] == newColor)
                {
                    currentColorIndex = i;
                    break;
                }
            }
        }
    }
    
    public void CycleColorForward()
    {
        currentColorIndex = (currentColorIndex + 1) % colorCycle.Length;
        SetColor(colorCycle[currentColorIndex]);
    }
    
    public void CycleColorBackward()
    {
        currentColorIndex = (currentColorIndex - 1 + colorCycle.Length) % colorCycle.Length;
        SetColor(colorCycle[currentColorIndex]);
    }
    
    public ResonanceColor GetCurrentColor()
    {
        return currentColor;
    }
    
    private int cachedFaceIndex = -1;
    private bool faceIndexFound = false;
    
    void UpdateVisuals()
    {
        if (sphereRenderer == null) return;
        
        // Cache the face index on first run to avoid repeated searches
        if (!faceIndexFound)
        {
            FindFaceIndex();
        }
        
        // Get current material to replace
        Material newMaterial = GetMaterialForColor(currentColor);
        if (newMaterial == null) return;
        
        // Store reference to current face material for pulse effects
        currentFaceMaterial = newMaterial;
        
        // Use sharedMaterials for better performance and avoid memory leaks
        Material[] materials = sphereRenderer.sharedMaterials;
        Material[] newMaterials = new Material[materials.Length];
        
        // Copy existing materials and replace only the face material
        for (int i = 0; i < materials.Length; i++)
        {
            newMaterials[i] = (i == cachedFaceIndex) ? newMaterial : materials[i];
        }
        
        // Apply the new materials array
        sphereRenderer.materials = newMaterials;
    }
    
    void FindFaceIndex()
    {
        Material[] materials = sphereRenderer.sharedMaterials;
        
        // Find which material slot contains "Face"
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i].name.ToLower().Contains("glow"))
            {
                cachedFaceIndex = i;
                faceIndexFound = true;
                return;
            }
        }
        
        // If no face material found, use first material slot
        cachedFaceIndex = 0;
        faceIndexFound = true;
    }
    
    Material GetMaterialForColor(ResonanceColor color)
    {
        switch (color)
        {
            case ResonanceColor.Red: return redFaceMaterial;
            case ResonanceColor.Green: return greenFaceMaterial;
            case ResonanceColor.Blue: return blueFaceMaterial;
            default: return whiteFaceMaterial;
        }
    }
    
    void HandleColorTransition()
    {
        // Simple instant transition for now - could be made smoother later
        currentColor = targetColor;
        UpdateVisuals();
        isTransitioning = false;
    }
    
    void HandlePulseEffect()
    {
        // Create a gentle pulse effect on the current face material
        if (currentFaceMaterial != null && currentFaceMaterial.HasProperty("_EmissionColor"))
        {
            // Use pulseIntensity with adjusted range to maintain higher minimum intensity
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + (1f + pulseIntensity);
            Color emissionColor = GetEmissionColorForResonanceColor(currentColor);
            currentFaceMaterial.SetColor("_EmissionColor", emissionColor * pulse);
        }
    }
    
    Color GetEmissionColorForResonanceColor(ResonanceColor color)
    {
        switch (color)
        {
            case ResonanceColor.Red: return new Color(1f, 0.27f, 0f, 1f); // Hot Orange
            case ResonanceColor.Green: return Color.magenta;
            case ResonanceColor.Blue: return Color.cyan;
            default: return Color.white;
        }
    }

}