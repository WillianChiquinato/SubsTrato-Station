using UnityEngine;

public class GeradorActive : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Itens") && other.GetComponent<Rigidbody>() != null)
        {
            // Ativar o gerador.
            GetComponentInParent<OrbSpawner>().StartPuzzle();
            Destroy(other.gameObject);

            ToastMessage.Instance.ShowToast("Chip ativado! Pegue as Orbes!!!!", ToastType.Success);
        }
    }
}
