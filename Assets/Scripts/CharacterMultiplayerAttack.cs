using UnityEngine;
using Fusion;

public class CharacterMultiplayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float knockbackForce = 5f;
    [SerializeField] private int damage = 10;

    [Networked] private bool hasHit { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Ignora se colidir com o próprio player
        NetworkObject otherNetworkObj = other.GetComponent<NetworkObject>();
        NetworkObject myNetworkObj = GetComponent<NetworkObject>();
        if (otherNetworkObj != null && otherNetworkObj == myNetworkObj) return;

        // Tenta pegar Health do alvo
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth == null) return;

        // Calcula direção do knockback
        Vector3 knockbackDir = (other.transform.position - transform.position).normalized;

        // Só quem tem autoridade do alvo aplica dano e knockback
        NetworkObject targetNetworkObj = other.GetComponent<NetworkObject>();
        if (targetNetworkObj != null && targetNetworkObj.HasStateAuthority)
        {
            KnockReceptor knock = other.GetComponent<KnockReceptor>();
            if (knock != null)
            {
                knock.ApplyKnockback(knockbackDir, knockbackForce);
            }

            targetHealth.TakeDamage(damage);

            // RPC para animação de hit
            RPC_PlayHitAnimation(targetNetworkObj);
        }

        hasHit = true;
    }


    // RPC para reproduzir animação de hit em todos os clientes.
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_PlayHitAnimation(NetworkObject target)
    {
        if (target == null) return;
        Animator targetAnimator = target.GetComponentInChildren<Animator>();
        if (targetAnimator != null)
        {
            targetAnimator.SetTrigger("Hit");
        }
    }

    // Reseta o ataque para poder bater novamente.
    public void ResetAttack()
    {
        hasHit = false;
    }
}
