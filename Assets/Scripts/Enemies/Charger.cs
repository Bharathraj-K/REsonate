using UnityEngine;

/// <summary>
/// Charger enemy - fast-moving with low health, rushes at the player aggressively
/// </summary>
public class Charger : Enemy
{
    [Header("Charger Settings")]
    public float rushDistance = 8f; // Distance at which charger starts rushing
    public float rushSpeedMultiplier = 3f; // Speed boost when rushing
    public float collisionDistance = 1f;
    
    private bool isRushing = false;
    private float originalMoveSpeed;
    
    protected override void Start()
    {
        // Set charger specific stats - fast but fragile
        moveSpeed = 4f;
        health = 0.5f; // Lower health than basic nodes
        originalMoveSpeed = moveSpeed;
        
        // Call base start
        base.Start();
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Check if should start rushing
        if (playerTransform != null && !isDestroyed)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // Start rushing when close enough
            if (!isRushing && distanceToPlayer <= rushDistance)
            {
                StartRush();
            }
            
            // Check for collision with player
            if (distanceToPlayer <= collisionDistance)
            {
                OnPlayerCollision();
            }
        }
    }
    
    private void StartRush()
    {
        isRushing = true;
        moveSpeed = originalMoveSpeed * rushSpeedMultiplier;
        
        // Visual feedback - could add particle effects here later
        // For now, we'll just increase the speed
    }
    
    protected override void MoveTowardsPlayer()
    {
        if (playerTransform == null) return;

        // More aggressive movement for charger
        Vector3 moveDir = (playerTransform.position - transform.position).normalized;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Use stronger force for more aggressive movement
            float forceMultiplier = isRushing ? 2f : 1f;
            rb.AddForce(moveDir * moveSpeed * forceMultiplier * Time.deltaTime, ForceMode.VelocityChange);
            
            // Limit max velocity to prevent crazy speeds
            if (rb.linearVelocity.magnitude > moveSpeed * 1.5f)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed * 1.5f;
            }
        }
        else
        {
            // Fallback transform movement
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }
    }
    
    protected override void OnPlayerCollision()
    {
        // Chargers deal damage when they hit the player
        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }
        
        // Destroy the charger after hitting the player
        DestroyEnemy();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw rush detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rushDistance);
        
        // Draw collision range
        Gizmos.color = new Color(1f, 0.27f, 0f, 1f); // Hot Orange
        Gizmos.DrawWireSphere(transform.position, collisionDistance);
    }
}