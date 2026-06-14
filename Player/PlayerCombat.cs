using System.Collections;
using UnityEngine;

/// <summary>
/// Emberpath - Core Player Combat (Prototype v0.1)
/// Handles: Attack input, hitbox activation, hit detection, hitstop (frame-freeze on hit).
/// Attach to the same GameObject as PlayerController.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    [Tooltip("Damage dealt per hit")]
    public int attackDamage = 10;
    [Tooltip("Time the hitbox stays active after the attack starts")]
    public float hitboxActiveTime = 0.12f;
    [Tooltip("Cooldown between attacks")]
    public float attackCooldown = 0.35f;
    [Tooltip("Knockback force applied to enemies on hit")]
    public float knockbackForce = 6f;

    [Header("Hitbox")]
    [Tooltip("Transform marking the center of the attack hitbox (offset in front of the player)")]
    public Transform hitboxOrigin;
    [Tooltip("Radius of the attack hitbox")]
    public float hitboxRadius = 0.6f;
    [Tooltip("Layers that can be hit by this attack")]
    public LayerMask enemyLayer;

    [Header("Hitstop (Game Feel)")]
    [Tooltip("How long time briefly freezes on a successful hit - makes attacks feel heavy")]
    public float hitstopDuration = 0.05f;
    [Tooltip("Time.timeScale value during hitstop (0 = full freeze)")]
    public float hitstopTimeScale = 0.02f;

    private float attackCooldownTimer;
    private bool isAttacking;

    private void Update()
    {
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.unscaledDeltaTime;
        }

        if (Input.GetButtonDown("Fire1") && attackCooldownTimer <= 0f && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        attackCooldownTimer = attackCooldown;

        // Small delay to sync with future animation wind-up.
        yield return new WaitForSeconds(0.05f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(GetHitboxPosition(), hitboxRadius, enemyLayer);

        bool didHit = false;

        foreach (Collider2D hit in hits)
        {
            DummyEnemy enemy = hit.GetComponent<DummyEnemy>();
            if (enemy != null)
            {
                Vector2 knockDir = (hit.transform.position - transform.position).normalized;
                enemy.TakeHit(attackDamage, knockDir * knockbackForce);
                didHit = true;
            }
        }

        if (didHit)
        {
            yield return StartCoroutine(Hitstop());
        }

        yield return new WaitForSeconds(hitboxActiveTime);
        isAttacking = false;
    }

    private IEnumerator Hitstop()
    {
        float originalScale = Time.timeScale;
        Time.timeScale = hitstopTimeScale;

        // Use unscaled time so the freeze itself isn't affected by timeScale.
        yield return new WaitForSecondsRealtime(hitstopDuration);

        Time.timeScale = originalScale;
    }

    private Vector2 GetHitboxPosition()
    {
        if (hitboxOrigin != null)
        {
            return hitboxOrigin.position;
        }

        // Fallback: project hitbox in front of the player based on facing direction.
        float facing = Mathf.Sign(transform.localScale.x);
        return (Vector2)transform.position + new Vector2(facing * 0.75f, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetHitboxPosition(), hitboxRadius);
    }
}
