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
        // Quando começar a segurar botão direito
        if (inputData.aimActive && !carregando)
        {
            carregando = true;
            forcaAtual = forcaMin;
        }

        // Enquanto segurar botão direito
        if (carregando && inputData.prepareArremessoPressed)
        {
            forcaAtual += velocidadeCarregamento * Runner.DeltaTime;
            forcaAtual = Mathf.Clamp(forcaAtual, forcaMin, forcaMax);

            // Só mostra trajetória no cliente local
            if (Object.HasInputAuthority)
            {
                MostrarTrajetoria();
            }
        }

        // Quando soltar botão direito
        if (inputData.arremessarPressed && carregando)
        {
            if (Object.HasStateAuthority)
            {
                ArremessarNoServidor();
            }
            else
            {
                // Cliente solicita arremesso ao servidor
                RPC_SolicitarArremesso(forcaAtual);
            }

            carregando = false;
            if (Object.HasInputAuthority)
            {
                lineRenderer.enabled = false;
            }
        }

        // Se parou de carregar sem arremessar
        if (carregando && !inputData.prepareArremessoPressed && !inputData.aimActive)
        {
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
        if (objetoArremessavelPrefab != null)
        {
            // USA A MESMA LÓGICA DO DROP QUE FUNCIONA!
            if (playerMoviment.inventory.myHandItem != null)
            {
                ItemClass itemClass = playerMoviment.inventory.myHandItem.GetComponent<ItemClass>();
                if (itemClass != null)
                {
                    // USA O MESMO PREFAB DO DROP QUE FUNCIONA!
                    GameObject dropPrefab = ItemDatabase.GetPrefabForItem(itemClass.itemSO);

                    Vector3 spawnPos = playerMoviment.playerCameraTransform.position + playerMoviment.playerCameraTransform.forward * 1.5f + Vector3.up * 0.5f;
                    var thrownObject = Runner.Spawn(dropPrefab, spawnPos, Quaternion.identity, Object.InputAuthority);

                    Debug.LogError("Item " + spawnPos + ", " + thrownObject.transform.position);

                    if (thrownObject.TryGetComponent<Rigidbody>(out var rb))
                    {
                        Vector3 direcaoMira = GetDirecaoMira();
                        rb.AddForce(direcaoMira * forcaAtual, ForceMode.Impulse);
                    }

                    RPC_RemoverItemDoInventario();
                }
            }
        }
    }

    private Vector3 GetDirecaoMira()
    {
        return playerMoviment.playerCameraTransform.forward;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RemoverItemDoInventario()
    {
        // Verifica se as referências estão válidas
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

    // Garante que a mira seja desativada quando o script for desativado
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
