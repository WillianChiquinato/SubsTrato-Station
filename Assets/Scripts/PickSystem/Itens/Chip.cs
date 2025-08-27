using UnityEngine;
using UnityEngine.Events;

public class Chip : MonoBehaviour
{
    [field: SerializeField] public UnityEvent OnUse { get; private set; }
    public ItemClass itemClass { get; private set; }

    public void Use(GameObject actor)
    {
        OnUse?.Invoke();
    }

    public void SoundItem()
    {
        Debug.Log("Play Sound");
    }
}
