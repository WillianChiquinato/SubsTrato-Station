using UnityEngine;

public class CharacterMultiplayerAttack : MonoBehaviour
{
    public float knockbackForce;

    [SerializeField]
    private int damage;
    public bool hasHit = false;

    private void OnTriggerEnter(Collider objectCollider)
    {
        if (hasHit) return;

        if (objectCollider.CompareTag("Player"))
        {
            hasHit = true;

            // Verificando se o jogador usa o CharacterController
            CharacterMultiplayer ColisorHit = objectCollider.GetComponent<CharacterMultiplayer>();
            Animator playerAnimator = objectCollider.GetComponentInChildren<Animator>();

            if (ColisorHit != null)
            {
                Vector3 knockbackDir = (objectCollider.transform.position - transform.position).normalized;
                ColisorHit.ApplyKnockback(knockbackDir, knockbackForce);

                Health targetHealth = objectCollider.GetComponent<Health>();
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
