using UnityEngine;

public class WeaponScaleAdjuster : MonoBehaviour
{
    [Header("Configurações de Escala")]
    public KeyCode increaseScaleKey = KeyCode.Plus;
    public KeyCode decreaseScaleKey = KeyCode.Minus;
    public KeyCode resetScaleKey = KeyCode.R;
    public float scaleStep = 0.1f;
    
    private PlayerInventory inventory;
    
    void Start()
    {
        inventory = FindFirstObjectByType<PlayerInventory>();
    }
    
    void Update()
    {
        if (inventory?.myHandItem == null) return;
        
        // Aumentar escala
        if (Input.GetKeyDown(increaseScaleKey))
        {
            AdjustScale(1f + scaleStep);
        }
        
        // Diminuir escala
        if (Input.GetKeyDown(decreaseScaleKey))
        {
            AdjustScale(1f - scaleStep);
        }
        
        // Reset para escala padrão
        if (Input.GetKeyDown(resetScaleKey))
        {
            ResetToDefaultScale();
        }
    }
    
    void AdjustScale(float multiplier)
    {
        Vector3 currentScale = inventory.myHandItem.transform.localScale;
        Vector3 newScale = currentScale * multiplier;
        
        // Limita a escala entre 0.5 e 3.0
        newScale = Vector3.Max(newScale, Vector3.one * 0.5f);
        newScale = Vector3.Min(newScale, Vector3.one * 3.0f);
        
        inventory.myHandItem.transform.localScale = newScale;
        
        Debug.Log($"Escala da arma ajustada para: {newScale:F2}");
    }
    
    void ResetToDefaultScale()
    {
        var weapon = inventory.myHandItem.GetComponent<Weapon>();
        if (weapon != null)
        {
            Vector3 defaultScale = Vector3.one;
            
            switch (weapon.Type)
            {
                case WeaponType.Pistol:
                    defaultScale = new Vector3(1.2f, 1.2f, 1.2f);
                    break;
                case WeaponType.Shotgun:
                    defaultScale = new Vector3(1.3f, 1.3f, 1.3f);
                    break;
                case WeaponType.Target:
                    defaultScale = new Vector3(1.25f, 1.25f, 1.25f);
                    break;
            }
            
            inventory.myHandItem.transform.localScale = defaultScale;
            Debug.Log($"Escala resetada para padrão: {defaultScale}");
        }
    }
    
    void OnGUI()
    {
        if (inventory?.myHandItem != null)
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 120, 300, 100));
            GUILayout.Label($"ESCALA DA ARMA: {inventory.myHandItem.transform.localScale:F2}");
            GUILayout.Label($"+ = Aumentar | - = Diminuir | R = Reset");
            GUILayout.EndArea();
        }
    }
}