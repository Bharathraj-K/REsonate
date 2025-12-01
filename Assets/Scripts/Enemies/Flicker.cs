using UnityEngine;
using System.Collections;

/// <summary>
/// Flicker enemy - changes color every few seconds, making it harder to target consistently
/// </summary>
public class Flicker : Enemy
{
    [Header("Flicker Settings")]
    public float colorChangeInterval = 3f; // Time between color changes
    public float collisionDistance = 1f;
    public bool showColorChangeEffect = true;
    
    private Coroutine colorChangeCoroutine;
    private float nextColorChangeTime;
    
    protected override void Start()
    {
        // Set flicker specific stats - moderate speed and health
        moveSpeed = 2.5f;
        health = 1f;
        
        // Call base start
        base.Start();
        
        // Start color changing behavior
        StartColorChanging();
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Check for collision with player
        if (playerTransform != null && !isDestroyed)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= collisionDistance)
            {
                OnPlayerCollision();
            }
        }
    }
    
    private void StartColorChanging()
    {
        if (colorChangeCoroutine != null)
        {
            StopCoroutine(colorChangeCoroutine);
        }
        
        colorChangeCoroutine = StartCoroutine(ColorChangeLoop());
    }
    
    private IEnumerator ColorChangeLoop()
    {
        while (!isDestroyed)
        {
            yield return new WaitForSeconds(colorChangeInterval);
            
            if (!isDestroyed) // Double check in case destroyed during wait
            {
                ChangeToRandomColor();
            }
        }
    }
    
    private void ChangeToRandomColor()
    {
        // Get current color to avoid changing to the same color
        ResonanceColor currentColor = enemyColor;
        
        // Pick a different random color
        ResonanceColor[] colors = { ResonanceColor.Red, ResonanceColor.Green, ResonanceColor.Blue };
        ResonanceColor newColor;
        
        do
        {
            newColor = colors[Random.Range(0, colors.Length)];
        } 
        while (newColor == currentColor && colors.Length > 1); // Ensure different color if possible
        
        // Apply the new color
        SetColor(newColor);
        
        // Optional: Add visual effect for color change
        if (showColorChangeEffect)
        {
            ShowColorChangeEffect();
        }
    }
    
    private void ShowColorChangeEffect()
    {
        // Simple scale pulse effect to indicate color change
        // In a more polished version, this could be particles or other effects
        StartCoroutine(ColorChangeEffectCoroutine());
    }
    
    private IEnumerator ColorChangeEffectCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        float duration = 0.2f;
        float elapsed = 0f;
        
        // Scale up
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (duration / 2f);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }
        
        elapsed = 0f;
        
        // Scale back down
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (duration / 2f);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }
        
        // Ensure we end at original scale
        transform.localScale = originalScale;
    }
    
    protected override void OnPlayerCollision()
    {
        // Flickers damage the player when they touch
        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }
        
        // Destroy the enemy after damaging player
        DestroyEnemy();
    }
    
    public override void DestroyEnemy()
    {
        // Stop color changing when destroyed
        if (colorChangeCoroutine != null)
        {
            StopCoroutine(colorChangeCoroutine);
        }
        
        base.DestroyEnemy();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw collision range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, collisionDistance);
    }
}