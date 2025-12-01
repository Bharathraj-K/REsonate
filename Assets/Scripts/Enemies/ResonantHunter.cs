using UnityEngine;

/// <summary>
/// Resonant Hunter enemy - actively avoids the player's current color, making it harder to target
/// Changes behavior based on what color the player is currently using
/// </summary>
public class ResonantHunter : Enemy
{
    [Header("Resonant Hunter Settings")]
    public float collisionDistance = 1f;
    public float avoidanceRadius = 6f; // Radius at which it starts avoiding player color
    public float avoidanceForce = 3f; // How strongly it tries to avoid
    public float colorCheckInterval = 0.2f; // How often to check player color
    
    private ResonanceCore playerResonanceCore;
    private ResonanceColor lastPlayerColor = ResonanceColor.None;
    private float lastColorCheckTime;
    private bool isAvoiding = false;
    
    protected override void Start()
    {
        // Set resonant hunter specific stats - moderate health, smart movement
        moveSpeed = 2.2f;
        health = 1.2f; // Slightly tougher since it's harder to hit
        
        // Call base start first to ensure playerTransform is found
        base.Start();
        
        // Find player's resonance core for color checking
        FindPlayerResonanceCore();
    }
    
    private void FindPlayerResonanceCore()
    {
        if (playerTransform != null)
        {
            playerResonanceCore = playerTransform.GetComponent<ResonanceCore>();
            if (playerResonanceCore == null)
            {
                Debug.LogWarning("ResonantHunter: Player found but no ResonanceCore component!");
            }
        }
        else
        {
            // Try to find player by tag as fallback
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerResonanceCore = player.GetComponent<ResonanceCore>();
            }
            else
            {
                Debug.LogWarning("ResonantHunter: Could not find player!");
            }
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Periodically check player color
        if (Time.time - lastColorCheckTime >= colorCheckInterval)
        {
            CheckPlayerColor();
            lastColorCheckTime = Time.time;
        }
        
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
    
    private void CheckPlayerColor()
    {
        // Try to find ResonanceCore if we don't have it yet
        if (playerResonanceCore == null)
        {
            FindPlayerResonanceCore();
            return;
        }
        
        ResonanceColor currentPlayerColor = playerResonanceCore.GetCurrentColor();
        
        // If player color changed, update our behavior
        if (currentPlayerColor != lastPlayerColor)
        {
            Debug.Log($"ResonantHunter: Player color changed from {lastPlayerColor} to {currentPlayerColor}, Hunter color: {enemyColor}");
            lastPlayerColor = currentPlayerColor;
            UpdateAvoidanceBehavior();
        }
    }
    
    private void UpdateAvoidanceBehavior()
    {
        // Check if we should be avoiding the player based on color match
        bool wasAvoiding = isAvoiding;
        isAvoiding = (lastPlayerColor == enemyColor && lastPlayerColor != ResonanceColor.None);
        
        if (wasAvoiding != isAvoiding)
        {
            Debug.Log($"ResonantHunter: Avoidance behavior changed to {isAvoiding} (Player: {lastPlayerColor}, Hunter: {enemyColor})");
        }
        
        // If we're avoiding and too close, try to change our color
        if (isAvoiding && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < avoidanceRadius)
            {
                // More frequent color change when actively avoiding
                if (Random.Range(0f, 1f) < 0.5f) // 50% chance per check
                {
                    ChangeToAvoidanceColor();
                }
            }
        }
    }
    
    private void ChangeToAvoidanceColor()
    {
        // Change to a color that's different from the player's current color
        ResonanceColor[] allColors = { ResonanceColor.Red, ResonanceColor.Green, ResonanceColor.Blue };
        ResonanceColor[] availableColors = System.Array.FindAll(allColors, color => color != lastPlayerColor && color != enemyColor);
        
        // If no different colors available, just pick any color different from current
        if (availableColors.Length == 0)
        {
            availableColors = System.Array.FindAll(allColors, color => color != enemyColor);
        }
        
        if (availableColors.Length > 0)
        {
            ResonanceColor oldColor = enemyColor;
            ResonanceColor newColor = availableColors[Random.Range(0, availableColors.Length)];
            SetColor(newColor);
            
            Debug.Log($"ResonantHunter: Changed color from {oldColor} to {newColor} to avoid player color {lastPlayerColor}");
            
            // Show visual feedback for color change
            ShowAvoidanceEffect();
        }
    }
    
    private void ShowAvoidanceEffect()
    {
        // Simple visual effect to show the hunter is actively avoiding
        StartCoroutine(AvoidanceEffectCoroutine());
    }
    
    private System.Collections.IEnumerator AvoidanceEffectCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 0.8f; // Shrink slightly
        float duration = 0.15f;
        float elapsed = 0f;
        
        // Shrink
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (duration / 2f);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }
        
        elapsed = 0f;
        
        // Return to original size
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (duration / 2f);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
    
    protected override void MoveTowardsPlayer()
    {
        if (playerTransform == null) return;
        
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        Vector3 moveDirection = directionToPlayer;
        float currentDistanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // If we're avoiding and close enough, modify movement behavior
        if (isAvoiding && currentDistanceToPlayer < avoidanceRadius)
        {
            // Calculate avoidance vector (perpendicular to player direction)
            Vector3 avoidanceDirection = Vector3.Cross(directionToPlayer, Vector3.up).normalized;
            
            // Use a consistent side based on position to avoid jittery movement
            float sideChoice = Mathf.Sign(Vector3.Dot(transform.position - playerTransform.position, Vector3.right));
            avoidanceDirection *= sideChoice;
            
            // Stronger avoidance the closer we get
            float avoidanceStrength = Mathf.Clamp01(1f - (currentDistanceToPlayer / avoidanceRadius));
            
            // Blend between moving toward player and avoiding
            if (currentDistanceToPlayer < avoidanceRadius * 0.5f)
            {
                // Very close - mostly avoid, slightly retreat
                moveDirection = (-directionToPlayer * 0.3f + avoidanceDirection * avoidanceForce).normalized;
            }
            else
            {
                // Medium distance - circle around player
                moveDirection = (directionToPlayer * 0.4f + avoidanceDirection * avoidanceForce).normalized;
            }
            
            // Reduce speed when avoiding to make it more noticeable
            moveSpeed *= 0.7f;
        }
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(moveDirection * moveSpeed * Time.deltaTime, ForceMode.VelocityChange);
            
            // Limit velocity to prevent crazy speeds
            if (rb.linearVelocity.magnitude > moveSpeed * 1.2f)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed * 1.2f;
            }
        }
        else
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
        
        // Reset moveSpeed for next frame (in case it was modified)
        if (isAvoiding && currentDistanceToPlayer < avoidanceRadius)
        {
            moveSpeed = 2.2f; // Reset to original value
        }
    }
    
    protected override void OnPlayerCollision()
    {
        // Resonant hunters damage the player when they touch
        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }
        
        // Destroy the enemy after damaging player
        DestroyEnemy();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw collision range
        Gizmos.color = new Color(1f, 0.27f, 0f, 1f); // Hot Orange
        Gizmos.DrawWireSphere(transform.position, collisionDistance);
        
        // Draw avoidance range
        Gizmos.color = isAvoiding ? Color.orange : Color.gray;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
        
        // Draw movement direction if avoiding
        if (isAvoiding && playerTransform != null)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, directionToPlayer * 2f);
            
            // Draw avoidance direction
            Vector3 avoidanceDirection = Vector3.Cross(directionToPlayer, Vector3.up).normalized;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, avoidanceDirection * 3f);
        }
        
        // Show current status in scene view
        #if UNITY_EDITOR
        if (playerResonanceCore != null)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"Hunter: {enemyColor}\nPlayer: {lastPlayerColor}\nAvoiding: {isAvoiding}");
        }
        #endif
    }
}