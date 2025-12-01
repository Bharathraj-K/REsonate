using UnityEngine;

/// <summary>
/// Basic Node enemy - simple slow-moving enemy that uses NavMesh pathfinding
/// </summary>
public class BasicNode : Enemy
{
    [Header("Basic Node Settings")]
    public float collisionDistance = 1f;
    
    protected override void Start()
    {
        // Set basic node specific stats
        moveSpeed = 2f;
        health = 1f;
        
        // Call base start
        base.Start();
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
    
    protected override void OnPlayerCollision()
    {
        // Basic nodes damage the player when they touch
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
        // Draw collision radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionDistance);
    }
}