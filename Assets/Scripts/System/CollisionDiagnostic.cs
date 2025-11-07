using UnityEngine;

public class CollisionDiagnostic : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableVisualDebug = true;
    public bool logAllCollisions = true;
    public Color debugColor = Color.red;
    public float debugSphereSize = 0.2f;
    
    private void OnDrawGizmos()
    {
        if (!enableVisualDebug) return;
        
        var collider = GetComponent<Collider>();
        if (collider == null) return;
        
        Gizmos.color = debugColor;
        Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
        Gizmos.DrawSphere(transform.position, debugSphereSize);
        
        if (collider.isTrigger)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, debugSphereSize * 1.5f);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        var collider = GetComponent<Collider>();
        if (collider == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(collider.bounds.center, collider.bounds.size);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (logAllCollisions)
        {
            string message = $"TRIGGER ENTER - {gameObject.name} <- {other.name} (Tag: {other.tag})";
            ThrowDebugLogger.LogThrow(message);
            Debug.Log($"[COLLISION_DEBUG] {message}");
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (logAllCollisions)
        {
            string message = $"TRIGGER STAY - {gameObject.name} <- {other.name} (Tag: {other.tag})";
            ThrowDebugLogger.LogThrow(message);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (logAllCollisions)
        {
            string message = $"TRIGGER EXIT - {gameObject.name} <- {other.name} (Tag: {other.tag})";
            ThrowDebugLogger.LogThrow(message);
            Debug.Log($"[COLLISION_DEBUG] {message}");
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (logAllCollisions)
        {
            string message = $"COLLISION ENTER - {gameObject.name} <- {collision.gameObject.name} (Tag: {collision.gameObject.tag})";
            ThrowDebugLogger.LogThrow(message);
            Debug.Log($"[COLLISION_DEBUG] {message}");
        }
    }
    
    // Método para verificar o estado atual do objeto
    [ContextMenu("Check Object State")]
    public void CheckObjectState()
    {
        var collider = GetComponent<Collider>();
        var rigidbody = GetComponent<Rigidbody>();
        var pushDoor = GetComponent<PushDoor>();
        var networkedPushDoor = GetComponent<NetworkedPushDoor>();
        
        Debug.Log("=== OBJECT STATE DIAGNOSTIC ===");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"Tag: {tag}");
        Debug.Log($"Layer: {LayerMask.LayerToName(gameObject.layer)}");
        Debug.Log($"Active: {gameObject.activeSelf}");
        Debug.Log($"Position: {transform.position}");
        
        if (collider != null)
        {
            Debug.Log($"Collider: {collider.GetType().Name}");
            Debug.Log($"IsTrigger: {collider.isTrigger}");
            Debug.Log($"Enabled: {collider.enabled}");
            Debug.Log($"Bounds: {collider.bounds}");
        }
        else
        {
            Debug.Log("Collider: NONE");
        }
        
        if (rigidbody != null)
        {
            Debug.Log($"Rigidbody: Present");
            Debug.Log($"IsKinematic: {rigidbody.isKinematic}");
            Debug.Log($"UseGravity: {rigidbody.useGravity}");
            Debug.Log($"Velocity: {rigidbody.linearVelocity}");
        }
        else
        {
            Debug.Log("Rigidbody: NONE");
        }
        
        if (pushDoor != null)
        {
            Debug.Log($"PushDoor: Present (Doors: {(pushDoor.door != null ? pushDoor.door.Length : 0)})");
        }
        
        if (networkedPushDoor != null)
        {
            Debug.Log($"NetworkedPushDoor: Present");
        }
        
        Debug.Log("=== END DIAGNOSTIC ===");
        
        // Log também via ThrowDebugLogger
        ThrowDebugLogger.LogThrow($"Diagnóstico completo para {gameObject.name} - Ver console para detalhes");
    }
}