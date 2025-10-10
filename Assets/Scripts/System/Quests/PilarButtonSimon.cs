using UnityEngine;
using UnityEngine.Events;

public class PilarButtonSimon : MonoBehaviour, IUsable
{
    [field: SerializeField] public UnityEvent OnUse { get; private set; }
    public GameObject SimonGameObject;
    public bool simonPilarReset = false;

    void Start()
    {
        simonPilarReset = true;
    }

    public void Use(GameObject actor)
    {
        OnUse?.Invoke();
    }

    void Update()
    {
        if (simonPilarReset)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Pickable");
        }
    }

    public void OnClickSimonGame()
    {
        SimonGameObject.GetComponent<SimonGame>().StartCoroutine(SimonGameObject.GetComponent<SimonGame>().StartRound());
        simonPilarReset = true;
    }
}
