using UnityEngine;

public class PushObject : MonoBehaviour
{
    public float pushPower = 2.0f;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Cadeira"))
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            // Se não tiver rigidbody ou for kinematic, não faz nada
            if (body == null || body.isKinematic)
                return;

            // Não empurrar no Y (pra não jogar a cadeira pra cima)
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            // Aplica a velocidade
            body.linearVelocity = pushDir * pushPower;
        }
    }
}
