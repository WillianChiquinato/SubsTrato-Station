using UnityEngine;
using Fusion;

public class ItemArremessavel : NetworkBehaviour
{
    [Networked] private Vector3 NetworkedPosition { get; set; }
    [Networked] private Quaternion NetworkedRotation { get; set; }
    [Networked] private Vector3 NetworkedVelocity { get; set; }
    [Networked] private Vector3 NetworkedAngularVelocity { get; set; }
    [Networked] private TickTimer InitialSyncTimer { get; set; }
    [Networked] private bool HasBeenInitialized { get; set; }

    private Rigidbody _rb;
    private bool _receivedInitialState;
    private float _lastSyncTime;

    public override void Spawned()
    {
        Debug.Log($"[ITEM_ARREMESSAVEL] Spawned - HasStateAuthority: {Object.HasStateAuthority}");
        
        _rb = GetComponent<Rigidbody>();
        
        // Remove qualquer NetworkTransform para evitar conflitos
        var netTransform = GetComponent<NetworkTransform>();
        if (netTransform != null)
        {
            DestroyImmediate(netTransform);
        }

        // Configura o rigidbody para melhor estabilidade
        if (_rb != null)
        {
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.useGravity = true;
            
            // Em builds, força a sincronização imediata
            if (!Application.isEditor)
            {
                _rb.isKinematic = Object.HasStateAuthority ? false : true;
            }
        }

        // CORREÇÃO CIRCLETRAIN: Define a tag "Target" para interação com PushDoor
        gameObject.tag = "Target";
        Debug.Log($"[ITEM_ARREMESSAVEL] {gameObject.name} configurado com tag Target");

        // Timer para garantir sincronização inicial
        if (Object.HasStateAuthority)
        {
            InitialSyncTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
            HasBeenInitialized = false;
        }

        _lastSyncTime = Time.time;
    }

    public override void FixedUpdateNetwork()
    {
        // Autoridade do estado sincroniza continuamente
        if (Object.HasStateAuthority)
        {
            if (_rb != null && HasBeenInitialized)
            {
                NetworkedPosition = _rb.position;
                NetworkedRotation = _rb.rotation;
                NetworkedVelocity = _rb.linearVelocity;
                NetworkedAngularVelocity = _rb.angularVelocity;
            }
            
            // Timer inicial expirou, marca como inicializado
            if (InitialSyncTimer.ExpiredOrNotRunning(Runner))
            {
                HasBeenInitialized = true;
            }
        }
        // Clientes aplicam o estado recebido
        else if (HasBeenInitialized && _rb != null)
        {
            // Em builds, usa interpolação mais suave
            float lerpSpeed = Application.isEditor ? 10f : 15f;
            
            if (!Application.isEditor)
            {
                // Em builds, força posição diretamente quando necessário
                float distance = Vector3.Distance(_rb.position, NetworkedPosition);
                if (distance > 0.5f) // Se muito longe, teleporta
                {
                    _rb.position = NetworkedPosition;
                    _rb.rotation = NetworkedRotation;
                }
                else
                {
                    // Interpolação suave
                    _rb.MovePosition(Vector3.Lerp(_rb.position, NetworkedPosition, Runner.DeltaTime * lerpSpeed));
                    _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, NetworkedRotation, Runner.DeltaTime * lerpSpeed));
                }
            }
            else
            {
                // No editor, usa interpolação normal
                _rb.MovePosition(Vector3.Lerp(_rb.position, NetworkedPosition, Runner.DeltaTime * lerpSpeed));
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, NetworkedRotation, Runner.DeltaTime * lerpSpeed));
            }
            
            _rb.linearVelocity = NetworkedVelocity;
            _rb.angularVelocity = NetworkedAngularVelocity;
        }
    }

    // Método para forçar estado inicial IMEDIATAMENTE
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_SetInitialState(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
    {
        Debug.Log($"[ITEM_ARREMESSAVEL] RPC Initial State - Position: {position}, Velocity: {velocity}");
        
        if (_rb != null)
        {
            // Força aplicação imediata
            _rb.position = position;
            _rb.rotation = rotation;
            _rb.linearVelocity = velocity;
            _rb.angularVelocity = angularVelocity;
            
            // Aplica também no transform para garantir
            transform.position = position;
            transform.rotation = rotation;
            
            // Em builds, força a física imediatamente
            if (!Application.isEditor && !Object.HasStateAuthority)
            {
                _rb.isKinematic = false;
            }
        }

        // Atualiza valores de rede
        if (Object.HasStateAuthority)
        {
            NetworkedPosition = position;
            NetworkedRotation = rotation;
            NetworkedVelocity = velocity;
            NetworkedAngularVelocity = angularVelocity;
            HasBeenInitialized = true;
        }
        
        _receivedInitialState = true;
        Debug.Log($"[ITEM_ARREMESSAVEL] Estado inicial aplicado com sucesso");
    }
}