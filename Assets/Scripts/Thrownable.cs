using UnityEngine;
using Fusion;

[RequireComponent(typeof(LineRenderer))]
public class Thrownable : NetworkBehaviour
{
    public PlayerMoviment playerMoviment;

    [Header("Arremesso")]
    public GameObject objetoArremessavelPrefab;
    public Transform spawnPoint;
    public float forcaMin = 5f;
    public float forcaMax = 20f;
    public float velocidadeCarregamento = 10f;

    [Header("Mira (trajetória)")]
    public int pontos = 30;
    public float tempoEntrePontos = 0.1f;

    private float forcaAtual;
    public bool carregando;
    public LineRenderer lineRenderer;

    void Start()
    {
        playerMoviment = GetComponentInParent<PlayerMoviment>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = pontos;
    }

    public override void FixedUpdateNetwork()
    {
        if (!playerMoviment.networkObject.HasInputAuthority) return;
        if (!GetInput(out NetworkInputData inputData)) return;

        // Verifica se tem item arremessável na mão
        if (playerMoviment.inventory.myHandItem != null &&
            playerMoviment.inventory.myHandItem.GetComponent<ItemArremessavel>() != null)
        {
            ProcessarArremesso(inputData);
        }
        else
        {
            // Desativa a mira se não tiver item
            if (carregando)
            {
                carregando = false;
                lineRenderer.enabled = false;
            }
        }
    }

    private void ProcessarArremesso(NetworkInputData inputData)
    {
        if (inputData.prepareArremessoPressed && !carregando && playerMoviment.Arremessar)
        {
            carregando = true;
            forcaAtual = forcaMin;
            ThrowDebugLogger.LogThrow($"Thrownable iniciou carregamento - Força inicial: {forcaAtual}");
            Debug.Log("[THROWNABLE] Iniciou carregamento do arremesso");
        }

        if (carregando && inputData.prepareArremessoPressed && playerMoviment.Arremessar)
        {
            forcaAtual += velocidadeCarregamento * Runner.DeltaTime;
            forcaAtual = Mathf.Clamp(forcaAtual, forcaMin, forcaMax);

            if (Object.HasInputAuthority)
            {
                MostrarTrajetoria();
            }
        }

        // Quando soltar botão direito (arremessar)
        if (inputData.arremessarPressed && carregando && !playerMoviment.Arremessar)
        {
            ThrowDebugLogger.LogThrow($"Executando arremesso - Força: {forcaAtual}, HasStateAuthority: {Object.HasStateAuthority}");
            Debug.Log($"[THROWNABLE] Executando arremesso com força: {forcaAtual}");
            
            if (Object.HasStateAuthority)
            {
                ArremessarNoServidor();
            }
            else
            {
                ThrowDebugLogger.LogNetworkEvent("RPC_REQUEST", $"Cliente solicitando arremesso com força {forcaAtual}");
                RPC_SolicitarArremesso(forcaAtual);
            }

            carregando = false;
            if (Object.HasInputAuthority)
            {
                lineRenderer.enabled = false;
            }
        }

        if (carregando && (!inputData.prepareArremessoPressed || !playerMoviment.Arremessar))
        {
            ThrowDebugLogger.LogThrow("Arremesso cancelado");
            Debug.Log("[THROWNABLE] Cancelou arremesso");
            carregando = false;
            if (Object.HasInputAuthority)
            {
                lineRenderer.enabled = false;
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SolicitarArremesso(float forca)
    {
        forcaAtual = forca;
        ArremessarNoServidor();
    }

    private void ArremessarNoServidor()
    {
        ThrowDebugLogger.LogNetworkEvent("SERVER_THROW", "ArremessarNoServidor iniciado");
        Debug.Log("[THROWNABLE] ArremessarNoServidor iniciado");
        
        if (playerMoviment.inventory.myHandItem == null)
        {
            ThrowDebugLogger.LogThrowError("Nenhum item na mão para arremessar");
            Debug.LogWarning("[THROWNABLE] Nenhum item na mão para arremessar");
            return;
        }

        ItemClass itemClass = playerMoviment.inventory.myHandItem.GetComponent<ItemClass>();
        if (itemClass == null)
        {
            ThrowDebugLogger.LogThrowError("ItemClass não encontrado no item");
            Debug.LogError("[THROWNABLE] ItemClass não encontrado no item");
            return;
        }

        GameObject dropPrefab = ItemDatabase.GetPrefabForItem(itemClass.itemSO);
        if (dropPrefab == null)
        {
            ThrowDebugLogger.LogThrowError($"Prefab não encontrado no ItemDatabase para {itemClass.itemSO.name}");
            Debug.LogError("[THROWNABLE] Prefab não encontrado no ItemDatabase!");
            return;
        }

        ThrowDebugLogger.LogNetworkEvent("INVENTORY_CLEAR", "Removendo item do inventário antes do spawn");
        RPC_RemoverItemDoInventario();

        StartCoroutine(SpawnItemComDelay(dropPrefab));
    }

    private System.Collections.IEnumerator SpawnItemComDelay(GameObject dropPrefab)
    {
        yield return null;
        
        Vector3 spawnPos = GetSpawnPosition();
        Vector3 direcaoMira = GetDirecaoMira();
        Vector3 velocity = direcaoMira * forcaAtual;
        Vector3 angularVelocity = Random.insideUnitSphere * 7f;

        ThrowDebugLogger.LogPhysicsEvent(dropPrefab.name, spawnPos, velocity);
        Debug.Log($"[THROWNABLE] Spawnando item em: {spawnPos} com força: {forcaAtual}");

        // Spawn do objeto
        var thrownObject = Runner.Spawn(
            dropPrefab,
            spawnPos,
            Quaternion.identity,
            Object.InputAuthority
        );

        if (thrownObject == null)
        {
            ThrowDebugLogger.LogThrowError("Falha ao spawnar objeto!");
            Debug.LogError("[THROWNABLE] Falha ao spawnar objeto!");
            yield break;
        }

        ThrowDebugLogger.LogNetworkEvent("SPAWN_SUCCESS", $"Objeto {thrownObject.name} spawnado com sucesso");

        yield return null;

        if (thrownObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.position = spawnPos;
            rb.linearVelocity = velocity;
            rb.angularVelocity = angularVelocity;
            ThrowDebugLogger.LogPhysicsEvent(thrownObject.name, rb.position, rb.linearVelocity);
            Debug.Log($"[THROWNABLE] Física aplicada - Velocidade: {velocity}");
        }

        if (thrownObject.TryGetComponent<ItemArremessavel>(out var item))
        {
            ThrowDebugLogger.LogNetworkEvent("INITIAL_SYNC", "Enviando RPC de estado inicial");
            item.Rpc_SetInitialState(spawnPos, Quaternion.identity, velocity, angularVelocity);
        }

        ThrowDebugLogger.LogNetworkEvent("THROW_COMPLETE", "Arremesso concluído com sucesso");
        Debug.Log("[THROWNABLE] Arremesso concluído com sucesso");
    }

    private Vector3 GetSpawnPosition()
    {
        return spawnPoint.position;
    }

    private Vector3 GetDirecaoMira()
    {
        return playerMoviment.playerCameraTransform.forward;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RemoverItemDoInventario()
    {
        if (playerMoviment == null || playerMoviment.inventory == null)
        {
            Debug.LogWarning("Referências do inventário não encontradas no RPC");
            return;
        }

        if (playerMoviment.inventory.myHandItem != null)
        {
            Destroy(playerMoviment.inventory.myHandItem);
            if (playerMoviment.inventory.selectedSlot >= 0 &&
                playerMoviment.inventory.selectedSlot < playerMoviment.inventory.hotbarItems.Length)
            {
                playerMoviment.inventory.hotbarItems[playerMoviment.inventory.selectedSlot] = null;
            }

            playerMoviment.inventory.myHandItem = null;
            playerMoviment.inventory.UpdateHotbarUI();

            Debug.Log("Item removido do inventário via RPC");
        }
    }

    private void MostrarTrajetoria()
    {
        if (!lineRenderer) return;

        lineRenderer.enabled = true;

        Vector3 posInicial = spawnPoint.position;
        Vector3 direcaoMira = GetDirecaoMira();
        Vector3 velocidadeInicial = direcaoMira * forcaAtual;

        for (int i = 0; i < pontos; i++)
        {
            float tempo = i * tempoEntrePontos;
            Vector3 posicao = posInicial + velocidadeInicial * tempo;
            posicao.y += 0.5f * Physics.gravity.y * tempo * tempo;
            lineRenderer.SetPosition(i, posicao);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
