using UnityEngine;
using Fusion;

public class ItemArremessavel : NetworkBehaviour
{
    [Networked] private Vector3 NetworkPosition { get; set; }
    [Networked] private Quaternion NetworkRotation { get; set; }
    [Networked] private Vector3 NetworkVelocity { get; set; }

    private Rigidbody _rb;
    private bool _initialized = false;

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();

        // REMOVA qualquer NetworkTransform do prefab!
        var netTransform = GetComponent<NetworkTransform>();
        if (netTransform != null)
        {
            Debug.Log("DESTRUINDO NetworkTransform!");
            Destroy(netTransform);
        }

        if (Object.HasStateAuthority)
        {
            // FORÇA a posição inicial
            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;
            _initialized = true;

            //Animação inicial, girando igual um doido.
            if (_rb != null)
            {
                _rb.angularVelocity = Random.insideUnitSphere * 15f;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && _rb != null)
        {
            // Atualiza dados de rede (servidor)
            NetworkPosition = _rb.position;
            NetworkRotation = _rb.rotation;
            NetworkVelocity = _rb.linearVelocity;
        }
        else if (!Object.HasStateAuthority && _rb != null)
        {
            // Aplica dados de rede (clientes)
            _rb.MovePosition(NetworkPosition);
            _rb.MoveRotation(NetworkRotation);
            _rb.linearVelocity = NetworkVelocity;
        }
    }
}

