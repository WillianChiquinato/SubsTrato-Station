using UnityEngine;
using UnityEngine.Events;

public class CircleButton : MonoBehaviour, IUsable
{
    [field: SerializeField] public UnityEvent OnUse { get; private set; }

    private SimonGame game;
    private Material material;
    private Color originalColor;
    private Color highlightColor = Color.white;
    private bool isHighlighted = false;

    void Start()
    {
        material = GetComponent<Renderer>().material;
        game = GetComponentInParent<SimonGame>();
        
        originalColor = material.color;
        highlightColor = originalColor * 2f;
        highlightColor.a = 1f;
    }

    public void Use(GameObject actor)
    {
        OnUse?.Invoke();
    }

    public void OnClick()
    {
        if (game != null)
        {
            game.OnCirclePressed(this);
        }
    }

    public void Highlight()
    {
        if (material != null && !isHighlighted)
        {
            isHighlighted = true;
            material.color = highlightColor;
            
            try
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", highlightColor * 0.5f);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Erro ao ativar emission: {e.Message}");
            }
        }
    }

    public void Unhighlight()
    {
        if (material != null && isHighlighted)
        {
            isHighlighted = false;
            material.color = originalColor;
            
            try
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Erro ao desativar emission: {e.Message}");
            }
        }
    }
}