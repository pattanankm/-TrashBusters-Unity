using System.Collections;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float baseKnockbackForce = 15f;
    public float knockbackDuration = 0.2f;

    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private bool isKnockbackActive = false; // นำตัวแปรตรรกะนี้มาดักเงื่อนไขเพื่อเคลียร์ป้ายเตือน Warning ค้างคา

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ถ้าชนกับผู้เล่นคนอื่น และตัวเราเองไม่ได้อยู่ในสภาวะมึนกระเด็นอยู่ก่อน
        if (collision.gameObject.CompareTag("Player") && !isKnockbackActive)
        {
            if (collision.gameObject.TryGetComponent<Rigidbody2D>(out Rigidbody2D otherRb))
            {
                Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;

                float myMass = rb.mass;
                float otherMass = otherRb.mass;
                float finalForce = baseKnockbackForce * (otherMass / myMass);

                StartCoroutine(ApplyKnockbackRoutine(knockbackDirection * finalForce));
            }
        }
    }

    private IEnumerator ApplyKnockbackRoutine(Vector2 force)
    {
        isKnockbackActive = true;
        
        if (playerMovement != null) playerMovement.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        if (playerMovement != null) playerMovement.enabled = true;
        
        isKnockbackActive = false;
    }
}