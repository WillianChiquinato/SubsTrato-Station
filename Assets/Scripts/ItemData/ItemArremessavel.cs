using UnityEngine;
using Fusion;

public class ItemArremessavel : NetworkBehaviour
{
    [Networked] private Vector3 NetworkedPosition { get; set; }
    [Networked] private Quaternion NetworkedRotation { get; set; }
    [Networked] private Vector3 NetworkedVelocity { get; set; }
    [Networked] private TickTimer InitialSyncTimer { get; set; }

    private Rigidbody _rb;
    private bool _receivedInitialState;

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();
        
        // Remove qualquer NetworkTransform
        var netTransform = GetComponent<NetworkTransform>();
        if (netTransform != null)
        {
            DestroyImmediate(netTransform);
        }

        // Configura o rigidbody para ser mais estável
        if (_rb != null)
        {
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // Timer para garantir sincronização inicial
        if (Object.HasStateAuthority)
        {
            InitialSyncTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Sincronização inicial garantida
        if (Object.HasStateAuthority && InitialSyncTimer.ExpiredOrNotRunning(Runner))
        {
            if (_rb != null)
            {
                NetworkedPosition = _rb.position;
                NetworkedRotation = _rb.rotation;
                NetworkedVelocity = _rb.linearVelocity;
            }
        }

        // Clientes aplicam o estado
        if (!Object.HasStateAuthority && _rb != null)
        {
            // Interpolação suave
            _rb.MovePosition(Vector3.Lerp(_rb.position, NetworkedPosition, Runner.DeltaTime * 10f));
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, NetworkedRotation, Runner.DeltaTime * 10f));
            _rb.linearVelocity = NetworkedVelocity;
        }
    }

    // Método para forçar estado inicial IMEDIATAMENTE
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_SetInitialState(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
    {
        Debug.Log($"RPC Initial State: {position}");
        
        if (_rb != null)
        {
            _rb.position = position;
            _rb.rotation = rotation;
            _rb.linearVelocity = velocity;
            _rb.angularVelocity = angularVelocity;
            
            // Aplica imediatamente no transform também
            transform.position = position;
            transform.rotation = rotation;
        }

        if (Object.HasStateAuthority)
        {
            NetworkedPosition = position;
            NetworkedRotation = rotation;
            NetworkedVelocity = velocity;
        }
        
        _receivedInitialState = true;
    }
}