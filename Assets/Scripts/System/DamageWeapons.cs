using UnityEngine;

public class DamageWeapons : MonoBehaviour
{
    public float knockbackForce;

    [SerializeField]
    private int damage;
    public bool hasHit = false;

    private void OnTriggerEnter(Collider ObjectCollider)
    {
        if (hasHit) return;

        if (ObjectCollider.CompareTag("Boss"))
        {
            hasHit = true;

            // Verificando se o jogador usa o CharacterController
            Boss BossController = ObjectCollider.GetComponent<Boss>();
            Animator bossAnimator = ObjectCollider.GetComponentInChildren<Animator>();

            if (BossController != null)
            {
                Vector3 knockbackDir = (ObjectCollider.transform.position - transform.position).normalized;
                BossController.ApplyKnockback(knockbackDir, knockbackForce);

                Health targetHealth = ObjectCollider.GetComponent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(damage);
                }
            }

            if (bossAnimator != null)
            {
                bossAnimator.SetTrigger("Hit");
            }
            else
            {
                Debug.LogError("Animator não encontrado no boss");
            }
        }
        else if (ObjectCollider.CompareTag("Enemy"))
        {
            hasHit = true;

            // Verificando se o jogador usa o CharacterController
            Enemy enemyController = ObjectCollider.GetComponent<Enemy>();
            Animator enemyAnimator = ObjectCollider.GetComponentInChildren<Animator>();

            if (enemyController != null)
            {
                Vector3 knockbackDir = (ObjectCollider.transform.position - transform.position).normalized;
                enemyController.ApplyKnockback(knockbackDir, knockbackForce);

                Health targetHealth = ObjectCollider.GetComponent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(damage);
                }
            }

            if (enemyAnimator != null)
            {
                enemyAnimator.SetTrigger("Hit");
            }
            else
            {
                Debug.LogError("Animator não encontrado no inimigo");
            }
        }
        else
        {
            Debug.LogError("Interface IEnemy não encontrada no inimigo");
        }
    }

    public void ResetAttack()
    {
        hasHit = false;
    }
}
