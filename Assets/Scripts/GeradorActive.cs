using UnityEngine;

public class GeradorActive : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Itens") && other.GetComponent<Rigidbody>() != null)
        {
            // Ativar o gerador.
            GetComponentInParent<ChipSystem>().chipSystemCount++;
            Destroy(other.gameObject);

            ToastMessage.Instance.ShowToast("Chip ativado! { " + GetComponentInParent<ChipSystem>().chipSystemCount + " de 3}", ToastType.Success);
        }
    }
}
