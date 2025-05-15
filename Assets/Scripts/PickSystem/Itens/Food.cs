using UnityEngine;
using UnityEngine.Events;

public class Food : MonoBehaviour, IUsable
{
    [field: SerializeField] public UnityEvent OnUse {get; private set;}

    public void Use(GameObject actor)
    {
        //Logica dos itens, no caso a comida.
        // actor.GetComponent<PlayerMoviment>().Heal(_healAmount);
        OnUse?.Invoke();
        Destroy(gameObject);
    }
}
