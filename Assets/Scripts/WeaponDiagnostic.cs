using UnityEngine;
using TMPro;

public class WeaponDiagnostic : MonoBehaviour
{
    [Header("UI Debug")]
    public TextMeshProUGUI debugText;
    public Canvas debugCanvas;
    
    private Weapon currentWeapon;
    private PlayerInventory inventory;
    private PlayerMoviment player;
    
    void Start()
    {
        if (debugCanvas == null)
        {
            CreateDebugUI();
        }
        
        inventory = FindFirstObjectByType<PlayerInventory>();
        player = FindFirstObjectByType<PlayerMoviment>();
    }
    
    void CreateDebugUI()
    {
        // Cria canvas
        GameObject canvasObj = new GameObject("WeaponDebugCanvas");
        debugCanvas = canvasObj.AddComponent<Canvas>();
        debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        debugCanvas.sortingOrder = 100;
        
        // Adiciona GraphicRaycaster
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(debugCanvas.transform);
        
        debugText = textObj.AddComponent<TextMeshProUGUI>();
        debugText.text = "Weapon Debug";
        debugText.fontSize = 14;
        debugText.color = Color.yellow;
        
        RectTransform rectTransform = debugText.rectTransform;
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(10, -10);
        rectTransform.sizeDelta = new Vector2(400, 200);
        
        ThrowDebugLogger.LogThrow("UI de debug de armas criada");
    }
    
    void Update()
    {
        if (debugText == null || inventory == null) return;
        
        UpdateWeaponInfo();
    }
    
    void UpdateWeaponInfo()
    {
        string info = "=== WEAPON DIAGNOSTIC ===\n";
        
        if (inventory.myHandItem != null)
        {
            currentWeapon = inventory.myHandItem.GetComponent<Weapon>();
            
            if (currentWeapon != null)
            {
                info += $"Arma: {inventory.myHandItem.name}\n";
                info += $"Tipo: {currentWeapon.Type}\n";
                info += $"Munição: {currentWeapon.CurrentAmmo}/{currentWeapon.MaxAmmo}\n";
                info += $"Pos Local: {inventory.myHandItem.transform.localPosition:F2}\n";
                info += $"Rot Local: {inventory.myHandItem.transform.localRotation:F2}\n";
                info += $"Offset Normal: {currentWeapon.Offset:F2}\n";
                info += $"Offset Mira: {currentWeapon.AimOffset:F2}\n";
                
                if (player != null)
                {
                    info += $"Mira Ativa: {player.aimActive}\n";
                    info += $"Anim Mira: {player.aimAnimActive}\n";
                    info += $"Arremesso: {player.Arremessar}\n";
                }
                
                // Verifica se posição está problemática
                Vector3 pos = inventory.myHandItem.transform.localPosition;
                if (pos.y > 0.5f || pos.magnitude > 2f)
                {
                    info += "⚠️ POSIÇÃO PROBLEMÁTICA! ⚠️\n";
                }
                
                // Verifica se tem WeaponOffsetFixer
                var fixer = inventory.myHandItem.GetComponent<WeaponOffsetFixer>();
                info += $"Fixer: {(fixer != null ? "✓" : "✗")}\n";
            }
            else
            {
                info += $"Item: {inventory.myHandItem.name}\n";
                info += "Tipo: Não é arma\n";
                info += $"Pos Local: {inventory.myHandItem.transform.localPosition:F2}\n";
            }
        }
        else
        {
            info += "Nenhum item na mão\n";
        }
        
        info += $"Slot Selecionado: {inventory.selectedSlot}\n";
        
        debugText.text = info;
    }
    
    [ContextMenu("Toggle Debug UI")]
    public void ToggleDebugUI()
    {
        if (debugCanvas != null)
        {
            debugCanvas.gameObject.SetActive(!debugCanvas.gameObject.activeSelf);
        }
    }
    
    [ContextMenu("Fix Current Weapon")]
    public void FixCurrentWeapon()
    {
        if (inventory?.myHandItem != null)
        {
            var fixer = inventory.myHandItem.GetComponent<WeaponOffsetFixer>();
            if (fixer == null)
            {
                fixer = inventory.myHandItem.AddComponent<WeaponOffsetFixer>();
            }
            
            fixer.SendMessage("ApplyDefaultOffsets", SendMessageOptions.DontRequireReceiver);
            ThrowDebugLogger.LogThrow("Weapon fixada manualmente via diagnóstico");
        }
    }
}