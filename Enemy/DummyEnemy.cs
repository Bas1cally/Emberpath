using UnityEngine;

/// <summary>
/// Emberpath - Dummy Enemy (Prototype v0.1)
/// Minimal enemy for testing player combat feel: takes damage, gets knocked back,
/// flashes on hit, and is destroyed (or respawns) at 0 HP.
/// Attach to a GameObject with a Rigidbody2D and Collider2D on the "Enemy" layer.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class DummyEnemy : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 30;

    [Header("Knockback")]
    [Tooltip("How long the enemy ignores its own movement logic while being knocked back")]
    public float knockbackRecoveryTime = 0.25f;

    [Header("Feedback")]
    [Tooltip("Sprite renderer used for hit-flash feedback")]
    public SpriteRenderer spriteRenderer;
    public Color hitFlashColor = Color.white;
    public float hitFlashDuration = 0.08f;

    [Header("Respawn (test arena convenience)")]
    [Tooltip("If true, the dummy respawns with full health after a delay instead of being destroyed")]
    public bool respawnOnDeath = true;
    public float respawnDelay = 1.5f;

    private int currentHealth;
    private Rigidbody2D rb;
    private float knockbackTimer;
    private Vector3 spawnPosition;
    private Color originalColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        spawnPosition = transform.position;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Called by PlayerCombat when an attack hitbox connects.
    /// </summary>
    public void TakeHit(int damage, Vector2 knockback)
    {
        currentHealth -= damage;
        knockbackTimer = knockbackRecoveryTime;
        rb.linearVelocity = knockback;

        FlashOnHit();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void FlashOnHit()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        CancelInvoke(nameof(ResetColor));
        spriteRenderer.color = hitFlashColor;
        Invoke(nameof(ResetColor), hitFlashDuration);
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        if (respawnOnDeath)
        {
            gameObject.SetActive(false);
            Invoke(nameof(Respawn), respawnDelay);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Respawn()
    {
        currentHealth = maxHealth;
        transform.position = spawnPosition;
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(true);
    }
}
