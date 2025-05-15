using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour, IUsable
{
    [field: SerializeField] public UnityEvent OnUse { get; private set; }

    public void Use(GameObject actor)
    {
        OnUse?.Invoke();
    }
    
    public void DebugLog()
    {
        Debug.Log("Weapon used");
    }
}
