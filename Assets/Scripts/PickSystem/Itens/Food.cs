using UnityEngine;
using UnityEngine.Events;

public class Food : MonoBehaviour, IUsable
{
    [field: SerializeField] public UnityEvent OnUse { get; private set; }
    public ItemClass itemClass { get; private set; }
    public int _healthBoost;
    public int _staminaBoost;
    public float _boostingTiming;
    public AudioSource foodSound;

    void Start()
    {
        itemClass = GetComponent<ItemClass>();
    }

    public void Use(GameObject actor)
    {
        //Logica dos itens, no caso a comida.
        actor.GetComponent<PlayerMoviment>().Heal(_healthBoost);
        actor.GetComponent<PlayerMoviment>().BoostStamina(_staminaBoost, _boostingTiming);
        OnUse?.Invoke();
        Debug.Log("Chamando funcao de food");
    }

    public void DestroyItem()
    {
        if (foodSound != null)
        {
            AudioSource.PlayClipAtPoint(foodSound.clip, transform.position);
        }
        Destroy(gameObject, foodSound.clip.length);
    }
}
