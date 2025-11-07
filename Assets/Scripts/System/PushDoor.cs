using UnityEngine;

public class PushDoor : MonoBehaviour
{
    public GameObject[] door;
    private bool doorOpened = false;

    void Start()
    {
        // Verifica se o collider está configurado corretamente
        var collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            Debug.LogWarning($"[PUSHDOOR] {gameObject.name} - Collider não está configurado como trigger!");
        }
        
        ThrowDebugLogger.LogThrow($"PushDoor configurado em {gameObject.name} - Portas: {(door != null ? door.Length : 0)}");
    }

    void OnTriggerEnter(Collider other)
    {
        ThrowDebugLogger.LogThrow($"PushDoor OnTriggerEnter - Objeto: {other.name}, Tag: {other.tag}");
        
        if (other.CompareTag("Target") && !doorOpened)
        {
            OpenDoors();
        }
    }

    void OnTriggerStay(Collider other)
    {
        ThrowDebugLogger.LogThrow($"PushDoor OnTriggerStay - Objeto: {other.name}, Tag: {other.tag}");
        
        if (other.CompareTag("Target") && !doorOpened)
        {
            OpenDoors();
        }
    }

    private void OpenDoors()
    {
        if (door == null || door.Length == 0)
        {
            ThrowDebugLogger.LogThrowError("PushDoor: Nenhuma porta configurada!");
            return;
        }

        doorOpened = true;
        ThrowDebugLogger.LogThrow($"Abrindo {door.Length} porta(s)");
        
        foreach (GameObject portas in door)
        {
            if (portas == null) continue;

            Animator anim = portas.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Open");
                ThrowDebugLogger.LogThrow($"Trigger 'Open' enviado para {portas.name}");
            }
            else
            {
                ThrowDebugLogger.LogThrowWarning($"Animator não encontrado em {portas.name}");
            }
            
            BoxCollider bc = portas.GetComponent<BoxCollider>();
            if (bc != null)
            {
                bc.enabled = false;
                ThrowDebugLogger.LogThrow($"BoxCollider desabilitado em {portas.name}");
            }
        }
        
        Debug.Log("[PUSHDOOR] Portas abertas com sucesso!");
    }

    // Método para resetar o estado da porta (útil para debug)
    public void ResetDoor()
    {
        doorOpened = false;
        ThrowDebugLogger.LogThrow("Estado da porta resetado");
    }
}
