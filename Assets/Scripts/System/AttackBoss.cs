using UnityEngine;

public class AttackBoss : MonoBehaviour
{
    public float knockbackForce;

    [SerializeField]
    private int damage;
    public bool hasHit = false;

    private void OnTriggerEnter(Collider playerCollider)
    {
        if (hasHit) return;

        if (playerCollider.CompareTag("Player"))
        {
            hasHit = true;

            // Verificando se o jogador usa o CharacterController
            PlayerMoviment playerController = playerCollider.GetComponent<PlayerMoviment>();
            Animator playerAnimator = playerCollider.GetComponentInChildren<Animator>();

            if (playerController != null)
            {
                Vector3 knockbackDir = (playerCollider.transform.position - transform.position).normalized;
                playerController.ApplyKnockback(knockbackDir, knockbackForce);

                Health targetHealth = playerCollider.GetComponent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(damage);
                }
            }


            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Hit");
            }
            else
            {
                Debug.LogError("Animator não encontrado no player");
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
