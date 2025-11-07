using UnityEngine;
using Fusion;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class NetworkedPushDoor : NetworkBehaviour
{
    public GameObject[] doors;
    
    [Networked] public bool IsOpened { get; set; } = false;
    [Networked] public TickTimer OpenCooldown { get; set; }
    
    private bool hasTriggered = false;

    public override void Spawned()
    {
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
            if (collider is MeshCollider meshCollider)
            {
                meshCollider.convex = true;
            }
        }
        
        ThrowDebugLogger.LogNetworkEvent("NETWORKED_DOOR", $"Porta de rede spawnada - HasStateAuthority: {Object.HasStateAuthority}");
    }

    public override void FixedUpdateNetwork()
    {
        if (IsOpened && !hasTriggered)
        {
            OpenDoorsLocal();
            hasTriggered = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        ProcessTrigger(other);
    }

    void OnTriggerStay(Collider other)
    {
        ProcessTrigger(other);
    }

    private void ProcessTrigger(Collider other)
    {
        ThrowDebugLogger.LogThrow($"NetworkedPushDoor trigger - Objeto: {other.name}, Tag: {other.tag}, IsOpened: {IsOpened}");
        
        if (!other.CompareTag("Target") || IsOpened) return;
        if (Object.HasStateAuthority)
        {
            if (OpenCooldown.ExpiredOrNotRunning(Runner))
            {
                OpenCooldown = TickTimer.CreateFromSeconds(Runner, 1f);
                RPC_OpenDoor();
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OpenDoor()
    {
        ThrowDebugLogger.LogNetworkEvent("RPC_OPEN_DOOR", "Comando de abertura da porta recebido");
        
        if (Object.HasStateAuthority)
        {
            IsOpened = true;
        }
        
        OpenDoorsLocal();
    }

    private void OpenDoorsLocal()
    {
        if (doors == null || doors.Length == 0)
        {
            ThrowDebugLogger.LogThrowError("NetworkedPushDoor: Nenhuma porta configurada!");
            return;
        }

        ThrowDebugLogger.LogThrow($"Abrindo {doors.Length} porta(s) via NetworkedPushDoor");
        
        foreach (GameObject door in doors)
        {
            if (door == null) continue;

            Animator anim = door.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Open");
                ThrowDebugLogger.LogThrow($"Trigger 'Open' enviado para {door.name}");
            }
            
            BoxCollider bc = door.GetComponent<BoxCollider>();
            if (bc != null)
            {
                bc.enabled = false;
                ThrowDebugLogger.LogThrow($"BoxCollider desabilitado em {door.name}");
            }
        }
        
        Debug.Log("[NETWORKED_PUSHDOOR] Portas abertas com sucesso!");
    }

    // Método público para configurar as portas externamente
    public void SetDoors(GameObject[] doorArray)
    {
        doors = doorArray;
        ThrowDebugLogger.LogThrow($"NetworkedPushDoor configurado com {doorArray.Length} porta(s)");
    }
}