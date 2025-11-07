using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class SimonGame : NetworkBehaviour
{
    [Header("Círculos em ordem")]
    public List<CircleButton> circles;
    public CircleTrain[] circleTrain;
    public float showDelay = 0.7f;
    public int totalTurns = 8;
    public GameObject PilarBtn;

    private List<int> Sequence = new List<int>();
    private int currentIndex = 0;
    private int currentTurn = 0;
    private bool playerTurn = false;
    private bool isShowingSequence = false;
    private bool canClick = true;

    [Networked] public int NetworkedVictoryIndex { get; set; } = -1;
    [Networked] public bool GameCompleted { get; set; } = false;
    
    private bool hasAppliedVictoryState = false;

    void Start()
    {
        for (int i = 0; i < circles.Count; i++)
        {
            Debug.Log($"Círculo {i}: {circles[i].name}");
        }
        
        StartCoroutine(VerifyNetworkSetup());
    }

    IEnumerator VerifyNetworkSetup()
    {
        yield return new WaitForSeconds(1f);
        
        ThrowDebugLogger.LogThrow("=== NETWORK SETUP VERIFICATION ===");
        ThrowDebugLogger.LogThrow($"GameObject: {gameObject.name}");
        ThrowDebugLogger.LogThrow($"Has NetworkObject: {(GetComponent<NetworkObject>() != null)}");
        ThrowDebugLogger.LogThrow($"Object: {(Object != null ? "EXISTS" : "NULL")}");
        ThrowDebugLogger.LogThrow($"Runner: {(Runner != null ? "EXISTS" : "NULL")}");
        
        if (Object != null)
        {
            ThrowDebugLogger.LogThrow($"Object.IsValid: {Object.IsValid}");
            ThrowDebugLogger.LogThrow($"HasStateAuthority: {Object.HasStateAuthority}");
            ThrowDebugLogger.LogThrow($"HasInputAuthority: {Object.HasInputAuthority}");
        }
        
        if (Runner != null)
        {
            ThrowDebugLogger.LogThrow($"Runner.IsClient: {Runner.IsClient}");
            ThrowDebugLogger.LogThrow($"Runner.IsServer: {Runner.IsServer}");
            ThrowDebugLogger.LogThrow($"Runner.Mode: {Runner.Mode}");
        }
        
        ThrowDebugLogger.LogThrow("=== END VERIFICATION ===");
    }

    public override void FixedUpdateNetwork()
    {
        if (GameCompleted && NetworkedVictoryIndex >= 0 && !hasAppliedVictoryState)
        {
            ThrowDebugLogger.LogThrow($"FixedUpdateNetwork: Aplicando vitória - Índice {NetworkedVictoryIndex}");
            ApplyVictoryState();
            hasAppliedVictoryState = true;
        }
    }

    void Update()
    {
        if (GameCompleted && NetworkedVictoryIndex >= 0 && !hasAppliedVictoryState)
        {
            ThrowDebugLogger.LogThrow($"Update: Aplicando vitória como backup - Índice {NetworkedVictoryIndex}");
            ApplyVictoryState();
            hasAppliedVictoryState = true;
        }
    }

    private void ApplyVictoryState()
    {
        ThrowDebugLogger.LogThrow($"Aplicando estado de vitória - Índice: {NetworkedVictoryIndex}");
        
        circles[NetworkedVictoryIndex].Highlight();
        var hightLight = circles[NetworkedVictoryIndex].gameObject.GetComponent<HightLights>();

        if (hightLight != null)
        {
            Destroy(hightLight);
        }

        foreach (var item in circleTrain)
        {
            item.doorIndex = NetworkedVictoryIndex;

            var circleIndex = circles.IndexOf(circles[NetworkedVictoryIndex]);
            var targetCircle = item.circles[circleIndex].gameObject;
            
            if (targetCircle.GetComponent<PushDoor>() == null && targetCircle.GetComponent<NetworkedPushDoor>() == null)
            {
                if (Runner != null && Object.HasStateAuthority)
                {
                    var networkedPushDoor = targetCircle.AddComponent<NetworkedPushDoor>();
                    networkedPushDoor.SetDoors(item.DoorInitialGame);
                    
                    ThrowDebugLogger.LogNetworkEvent("NETWORKED_DOOR_SETUP", $"NetworkedPushDoor configurado no círculo {circleIndex}");
                }
                else
                {
                    var pushDoor = targetCircle.AddComponent<PushDoor>();
                    pushDoor.door = item.DoorInitialGame;
                    
                    var meshCollider = targetCircle.AddComponent<MeshCollider>();
                    meshCollider.isTrigger = true;
                    meshCollider.convex = true;
                    
                    ThrowDebugLogger.LogThrow($"PushDoor padrão configurado no círculo {circleIndex}");
                }
                
                // Log para debug
                ThrowDebugLogger.LogThrow($"Circle Train configurado: Círculo {circleIndex} no índice de vitória {NetworkedVictoryIndex}");
                Debug.Log($"[SIMON] Configurando PushDoor no círculo {circleIndex} (índice de vitória: {NetworkedVictoryIndex})");
            }
        }
    }

    public IEnumerator StartRound()
    {
        playerTurn = false;
        isShowingSequence = true;
        ToastMessage.Instance.ShowToast("Observe a sequência!", ToastType.Alert);

        if (currentTurn >= totalTurns)
        {
            Debug.Log("🏆 Você completou todas as 8 rodadas! Vitória!");
            ToastMessage.Instance.RemoveAllToast();
            StartCoroutine(ShowVictoryMessage());
            yield break;
        }

        yield return new WaitForSeconds(1.5f);

        Sequence.Add(Random.Range(0, circles.Count));
        currentTurn++;
        Debug.Log($"▶️ Turno {currentTurn}/{totalTurns}");

        for (int i = 0; i < Sequence.Count; i++)
        {
            circles[Sequence[i]].Highlight();
            yield return new WaitForSeconds(showDelay);
            circles[Sequence[i]].Unhighlight();
            yield return new WaitForSeconds(0.2f);
        }

        isShowingSequence = false;
        playerTurn = true;
        currentIndex = 0;
    }

    public void OnCirclePressed(CircleButton circle)
    {
        if (!playerTurn || isShowingSequence) return;
        if (!canClick) return;

        canClick = false;
        StartCoroutine(EnableClickDelay());

        int pressedIndex = circles.IndexOf(circle);
        StartCoroutine(FlashPressed(circle));

        if (pressedIndex == Sequence[currentIndex])
        {
            currentIndex++;
            if (currentIndex >= Sequence.Count)
            {
                playerTurn = false;
                StartCoroutine(DelayedNextRound());
                ToastMessage.Instance.ShowToast("Acertou a sequência, continua!", ToastType.Success);
            }
        }
        else
        {
            Debug.Log($"Errou na posição {currentIndex + 1}! Reiniciando...");
            playerTurn = false;
            isShowingSequence = false;
            StartCoroutine(ResetGame());
            ToastMessage.Instance.ShowToast("Errou a sequência, resetando...", ToastType.Error);
        }
    }

    private IEnumerator DelayedNextRound()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(StartRound());
    }

    private IEnumerator ResetGame()
    {
        yield return new WaitForSeconds(1.5f);
        Sequence.Clear();
        currentTurn = 0;
        currentIndex = 0;
        PilarBtn.GetComponent<PilarButtonSimon>().simonPilarReset = false;
    }

    private IEnumerator EnableClickDelay()
    {
        yield return new WaitForSeconds(0.25f);
        canClick = true;
    }

    IEnumerator FlashPressed(CircleButton circle)
    {
        circle.Highlight();
        yield return new WaitForSeconds(0.2f);
        circle.Unhighlight();
    }

    IEnumerator ShowVictoryMessage()
    {
        yield return new WaitForSeconds(1.5f);

        ToastMessage.Instance.ShowToast("Você completou todas as rodadas! Vitória!", ToastType.Success);
        PilarBtn.GetComponent<PilarButtonSimon>().simonPilarReset = true;
        Sequence.Clear();
        currentTurn = 0;
        playerTurn = false;
        isShowingSequence = false;

        // Debug para verificar o estado do objeto
        ThrowDebugLogger.LogThrow($"=== VICTORY DEBUG ===");
        ThrowDebugLogger.LogThrow($"Object: {(Object != null ? "EXISTS" : "NULL")}");
        ThrowDebugLogger.LogThrow($"Runner: {(Runner != null ? "EXISTS" : "NULL")}");
        ThrowDebugLogger.LogThrow($"HasStateAuthority: {(Object != null ? Object.HasStateAuthority.ToString() : "UNKNOWN")}");
        ThrowDebugLogger.LogThrow($"HasInputAuthority: {(Object != null ? Object.HasInputAuthority.ToString() : "UNKNOWN")}");
        ThrowDebugLogger.LogThrow($"IsValid: {(Object != null ? Object.IsValid.ToString() : "UNKNOWN")}");

        bool shouldSetVictory = false;
        
        if (Object != null && Object.HasStateAuthority)
        {
            shouldSetVictory = true;
            ThrowDebugLogger.LogThrow("Servidor definindo vitória (StateAuthority)");
        }
        else if (Object == null || Runner == null)
        {
            shouldSetVictory = true;
            ThrowDebugLogger.LogThrow("Definindo vitória localmente (modo offline ou problema de rede)");
        }
        else if (!Object.HasStateAuthority)
        {
            ThrowDebugLogger.LogThrow("Cliente aguardando sincronização do servidor...");
            yield return new WaitForSeconds(2f);
            
            if (!GameCompleted)
            {
                shouldSetVictory = true;
                ThrowDebugLogger.LogThrowWarning("Servidor não sincronizou - forçando vitória localmente");
            }
        }

        if (shouldSetVictory)
        {
            int victoryIndex = Random.Range(0, circles.Count);
            
            if (Object != null && Object.HasStateAuthority)
            {
                NetworkedVictoryIndex = victoryIndex;
                GameCompleted = true;
            }
            else
            {
                ApplyVictoryStateLocal(victoryIndex);
            }
            
            ThrowDebugLogger.LogNetworkEvent("SIMON_VICTORY", $"Índice de vitória definido: {victoryIndex}");
        }
    }

    // Método para aplicar vitória sem usar variáveis de rede
    private void ApplyVictoryStateLocal(int victoryIndex)
    {
        ThrowDebugLogger.LogThrow($"Aplicando estado de vitória LOCAL - Índice: {victoryIndex}");
        
        circles[victoryIndex].Highlight();
        var hightLight = circles[victoryIndex].gameObject.GetComponent<HightLights>();

        if (hightLight != null)
        {
            Destroy(hightLight);
        }

        foreach (var item in circleTrain)
        {
            item.doorIndex = victoryIndex;

            var circleIndex = circles.IndexOf(circles[victoryIndex]);
            var targetCircle = item.circles[circleIndex].gameObject;
            
            if (targetCircle.GetComponent<PushDoor>() == null && targetCircle.GetComponent<NetworkedPushDoor>() == null)
            {
                var pushDoor = targetCircle.AddComponent<PushDoor>();
                pushDoor.door = item.DoorInitialGame;
                
                var meshCollider = targetCircle.AddComponent<MeshCollider>();
                meshCollider.isTrigger = true;
                meshCollider.convex = true;
                
                ThrowDebugLogger.LogThrow($"PushDoor LOCAL configurado no círculo {circleIndex}");
            }
        }
        
        hasAppliedVictoryState = true;
    }

    [ContextMenu("Force Victory")]
    public void ForceVictory()
    {
        ThrowDebugLogger.LogThrow("FORÇANDO VITÓRIA MANUAL");
        
        if (Object != null && Object.HasStateAuthority)
        {
            NetworkedVictoryIndex = Random.Range(0, circles.Count);
            GameCompleted = true;
        }
        else
        {
            ApplyVictoryStateLocal(Random.Range(0, circles.Count));
        }
    }

    // Método para resetar o estado (útil para testes)
    [ContextMenu("Reset Victory State")]
    public void ResetVictoryState()
    {
        ThrowDebugLogger.LogThrow("RESETANDO ESTADO DE VITÓRIA");
        
        if (Object != null && Object.HasStateAuthority)
        {
            NetworkedVictoryIndex = -1;
            GameCompleted = false;
        }
        
        hasAppliedVictoryState = false;
        
        // Remove componentes adicionados dinamicamente
        foreach (var item in circleTrain)
        {
            foreach (var circle in item.circles)
            {
                var pushDoor = circle.GetComponent<PushDoor>();
                if (pushDoor != null) DestroyImmediate(pushDoor);
                
                var networkedPushDoor = circle.GetComponent<NetworkedPushDoor>();
                if (networkedPushDoor != null) DestroyImmediate(networkedPushDoor);
                
                var meshCollider = circle.GetComponent<MeshCollider>();
                if (meshCollider != null) DestroyImmediate(meshCollider);
            }
        }
    }
}