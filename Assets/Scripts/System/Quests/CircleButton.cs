using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CircleButton : MonoBehaviour, IUsable
{
    [field: SerializeField] public UnityEvent OnUse { get; private set; }

    private SimonGame game;
    private Material material;


    void Start()
    {
        material = GetComponent<Renderer>().material;
        game = GetComponentInParent<SimonGame>();
    }

    public void Use(GameObject actor)
    {
        OnUse?.Invoke();
    }

    public void OnClick()
    {
        game.OnCirclePressed(this);
    }

    public void Highlight()
    {
        material.EnableKeyword("_EMISSION");
    }

    public void Unhighlight()
    {
        material.DisableKeyword("_EMISSION");
    }
}
