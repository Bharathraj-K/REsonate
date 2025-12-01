using UnityEngine;

/// <summary>
/// Base class for all enemies - provides common functionality including
/// movement, health management, color assignment, and visual representation
/// </summary>
public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float moveSpeed = 2f;
    public float health = 1f;
    
    [Header("Visual Settings")]
    public ResonanceColor enemyColor = ResonanceColor.Red;
    public Material[] colorMaterials = new Material[3]; // Red, Green, Blue materials
    public int enemyFaceIdx = 0; // Index of the material to change on the enemy model
    
    protected Transform playerTransform;
    protected Renderer enemyRenderer;
    protected bool isDestroyed = false;
    
    // Events for enemy actions
    public System.Action<Enemy> OnEnemyDestroyed;
    
    protected virtual void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Get renderer component
        enemyRenderer = GetComponent<Renderer>();
        
        // Set initial color
        SetRandomColor();
        UpdateVisuals();
    }
    
    protected virtual void Update()
    {
        if (!isDestroyed && playerTransform != null)
        {
            MoveTowardsPlayer();
        }
    }
    
    protected virtual void MoveTowardsPlayer()
    {
        if (playerTransform == null) return;

        // Calculate direction FROM enemy TO player (corrected!)
        Vector3 moveDir = (playerTransform.position - transform.position).normalized;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(moveDir * moveSpeed * Time.deltaTime, ForceMode.VelocityChange);
        }
        else
        {
            Debug.LogWarning("Enemy needs a Rigidbody component for movement!");
        }
    }
    
    public virtual void SetColor(ResonanceColor newColor)
    {
        enemyColor = newColor;
        UpdateVisuals();
    }
    
    protected virtual void SetRandomColor()
    {
        // Pick a random color from the available colors
        ResonanceColor[] colors = { ResonanceColor.Red, ResonanceColor.Green, ResonanceColor.Blue };
        enemyColor = colors[Random.Range(0, colors.Length)];
    }
    
    protected virtual void UpdateVisuals()
    {
        if (enemyRenderer == null) return;
        
        Material materialToUse = null;
        switch (enemyColor)
        {
            case ResonanceColor.Red:
                materialToUse = colorMaterials[0];
                break;
            case ResonanceColor.Green:
                materialToUse = colorMaterials[1];
                break;
            case ResonanceColor.Blue:
                materialToUse = colorMaterials[2];
                break;
        }
        
        if (materialToUse != null)
        {
            // Get current materials array
            Material[] materials = enemyRenderer.materials;
            
            // Make sure the face index is valid
            if (enemyFaceIdx >= 0 && enemyFaceIdx < materials.Length)
            {
                // Replace only the face material
                materials[enemyFaceIdx] = materialToUse;
                
                // Apply the modified materials array back to the renderer
                enemyRenderer.materials = materials;
            }
            else
            {
                Debug.LogWarning($"Invalid enemyFaceIdx {enemyFaceIdx} for enemy with {materials.Length} materials");
            }
        }
        else
        {
            Debug.LogWarning($"No material found for color {enemyColor}. Make sure colorMaterials array is properly assigned.");
        }
    }
    
    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0 && !isDestroyed)
        {
            DestroyEnemy();
        }
    }
    
    public virtual void DestroyEnemy()
    {
        isDestroyed = true;
        OnEnemyDestroyed?.Invoke(this);
        
        // Phase 8: Enhanced destruction with effects
        Vector3 destroyPosition = transform.position;
        Color enemyDisplayColor = GetEnemyDisplayColor();
        
        // Play destruction effect
        if (EffectsManager.Instance != null)
        {
            EffectsManager.Instance.PlayEnemyDestruction(destroyPosition, enemyDisplayColor);
        }
        
        // Screen shake handled by EffectsManager
        // Audio handled by EffectsManager
        
        // Disable renderer to prevent pink sphere from showing during destruction delay
        if (enemyRenderer != null)
        {
            enemyRenderer.enabled = false;
        }
        
        // Disable collider to prevent further interactions
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        
        Destroy(gameObject, 0.2f); // Small delay for effects to play
    }
    
    private Color GetEnemyDisplayColor()
    {
        switch (enemyColor)
        {
            case ResonanceColor.Red: return new Color(1f, 0.27f, 0f, 1f); // Hot Orange
            case ResonanceColor.Green: return Color.magenta;
            case ResonanceColor.Blue: return Color.cyan;
            default: return Color.white;
        }
    }
    
    public ResonanceColor GetColor()
    {
        return enemyColor;
    }
    
    public bool IsDestroyed()
    {
        return isDestroyed;
    }
    
    // Abstract methods for different enemy behaviors
    protected abstract void OnPlayerCollision();
}