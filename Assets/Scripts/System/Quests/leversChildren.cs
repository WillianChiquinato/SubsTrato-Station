using UnityEngine;
using UnityEngine.Events;

public class leversChildren : MonoBehaviour, IUsable
{
    [field: SerializeField] public UnityEvent OnUse { get; private set; }
    public GameObject leverObject;

    public void Use(GameObject actor)
    {
        OnUse?.Invoke();
    }

    public void ActiveItem()
    {
        leverObject.GetComponent<levers>().Use();
        Debug.LogWarning("Usou a alavanca filha");
    }
}
