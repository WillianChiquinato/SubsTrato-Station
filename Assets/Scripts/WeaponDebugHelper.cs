using UnityEngine;
using TMPro;

public class WeaponDebugHelper : MonoBehaviour
{
    [Header("Auto Debug")]
    public bool enableDebugOverlay = true;
    public KeyCode toggleKey = KeyCode.F1;
    
    private PlayerInventory inventory;
    private PlayerMoviment player;
    public GameObject debugUI;
    private TextMeshProUGUI debugText;
    private int buildFixFrameCounter = 0;
    private const int BUILD_FIX_FRAMES = 10; // Força correção por 10 frames em builds
    
    void Start()
    {
        inventory = FindFirstObjectByType<PlayerInventory>();
        player = FindFirstObjectByType<PlayerMoviment>();
        
        if (enableDebugOverlay)
        {
            CreateDebugOverlay();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleDebugOverlay();
        }
        
        if (debugUI != null && debugUI.activeSelf)
        {
            UpdateDebugInfo();
        }
        
        // Auto-fix problemas detectados
        AutoFixWeaponIssues();
        
        // Em builds, força correção por alguns frames após equipar item
        if (!Application.isEditor && inventory?.myHandItem != null && buildFixFrameCounter < BUILD_FIX_FRAMES)
        {
            ForceBuildParentFix();
            buildFixFrameCounter++;
        }
        else if (inventory?.myHandItem == null)
        {
            buildFixFrameCounter = 0; // Reset counter quando não há item
        }
    }
    
    void CreateDebugOverlay()
    {
        debugUI = new GameObject("WeaponDebugOverlay");
        Canvas canvas = debugUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        
        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(debugUI.transform);
        
        debugText = textObj.AddComponent<TextMeshProUGUI>();
        debugText.fontSize = 12;
        debugText.color = Color.green;
        debugText.text = "Weapon Debug Active";
        
        RectTransform rect = debugText.rectTransform;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(10, 10);
        rect.sizeDelta = new Vector2(500, 300);
        
        debugUI.SetActive(enableDebugOverlay);
    }
    
    void ToggleDebugOverlay()
    {
        if (debugUI == null)
        {
            CreateDebugOverlay();
        }
        else
        {
            debugUI.SetActive(!debugUI.activeSelf);
        }
    }
    
    void UpdateDebugInfo()
    {
        if (debugText == null) return;
        
        string info = $"[WEAPON DEBUG] Pressione {toggleKey} para alternar\n";
        info += $"🔧 MODO: {(Application.isEditor ? "EDITOR" : "BUILD")}\n\n";
        
        if (inventory?.myHandItem != null)
        {
            var weapon = inventory.myHandItem.GetComponent<Weapon>();
            var throwable = inventory.myHandItem.GetComponent<ItemArremessavel>();
            var chip = inventory.myHandItem.GetComponent<Chip>();
            
            info += $"🔧 ITEM EQUIPADO: {inventory.myHandItem.name}\n";
            info += $"📍 Parent: {(inventory.myHandItem.transform.parent != null ? inventory.myHandItem.transform.parent.name : "NULL")}\n";
            info += $"🌍 Pos Mundial: {inventory.myHandItem.transform.position:F2}\n";
            info += $"📌 Pos Local: {inventory.myHandItem.transform.localPosition:F2}\n";
            info += $"📏 Escala: {inventory.myHandItem.transform.localScale:F2}\n";
            
            // Identifica o tipo de item
            if (throwable != null)
            {
                info += $"🎯 TIPO: Item Arremessável (Pé de Cabra)\n";
                
                if (player != null)
                {
                    info += $"🎯 Estado Arremesso: {player.Arremessar}\n";
                    
                    // Input de arremesso em tempo real
                    bool rightMousePressed = Input.GetMouseButton(1);
                    bool rightMouseUp = Input.GetMouseButtonUp(1);
                    info += $"🖱️ Botão Direito: Press={rightMousePressed}, Up={rightMouseUp}\n";
                    
                    if (player.Arremessar)
                    {
                        info += $"⚡ CARREGANDO ARREMESSO...\n";
                    }
                }
                
                var itemClass = inventory.myHandItem.GetComponent<ItemClass>();
                info += $"ItemClass: {(itemClass != null ? "✅" : "❌")}\n";
            }
            else if (weapon != null)
            {
                info += $"🔫 TIPO: Arma\n";
                info += $"Subtipo: {weapon.Type}\n";
                info += $"Munição: {weapon.CurrentAmmo}/{weapon.MaxAmmo}\n";
                info += $"Offset Normal: {weapon.Offset:F2}\n";
                info += $"Offset Mira: {weapon.AimOffset:F2}\n";
                
                // Status da mira
                if (player != null)
                {
                    info += $"🎯 Mira Ativa: {player.aimActive} / {player.aimAnimActive}\n";
                    
                    // Input de mira em tempo real
                    bool rightMousePressed = Input.GetMouseButton(1);
                    info += $"🖱️ Botão Direito: {rightMousePressed}\n";
                    
                    // Verifica se posição está correta
                    Vector3 expectedPos = player.aimActive ? weapon.AimOffset : weapon.Offset;
                    float distance = Vector3.Distance(inventory.myHandItem.transform.localPosition, expectedPos);
                    
                    if (distance > 0.1f)
                    {
                        info += $"⚠️ POSIÇÃO INCORRETA! Distância: {distance:F2}\n";
                        info += $"Esperado: {expectedPos:F2}\n";
                        info += $"Atual: {inventory.myHandItem.transform.localPosition:F2}\n";
                    }
                    else
                    {
                        info += $"✅ Posição correta\n";
                    }
                }
                
                // Verifica componentes
                var fixer = inventory.myHandItem.GetComponent<WeaponOffsetFixer>();
                info += $"WeaponOffsetFixer: {(fixer != null ? "✅" : "❌")}\n";
                
                var itemClass = inventory.myHandItem.GetComponent<ItemClass>();
                info += $"ItemClass: {(itemClass != null ? "✅" : "❌")}\n";
            }
            else if (chip != null)
            {
                info += $"🔌 TIPO: Chip\n";
                
                var itemClass = inventory.myHandItem.GetComponent<ItemClass>();
                info += $"ItemClass: {(itemClass != null ? "✅" : "❌")}\n";
            }
            else
            {
                info += "❓ TIPO: Desconhecido\n";
                
                var itemClass = inventory.myHandItem.GetComponent<ItemClass>();
                info += $"ItemClass: {(itemClass != null ? "✅" : "❌")}\n";
            }
        }
        else
        {
            info += "🤲 Nenhuma arma na mão\n";
        }
        
        // Input debug
        info += "\n📱 INPUT DEBUG:\n";
        info += $"Mouse 1 (Down): {Input.GetMouseButtonDown(1)}\n";
        info += $"Mouse 1 (Hold): {Input.GetMouseButton(1)}\n";
        info += $"Mouse 1 (Up): {Input.GetMouseButtonUp(1)}\n";
        
        debugText.text = info;
    }
    
    void AutoFixWeaponIssues()
    {
        if (inventory?.myHandItem == null) return;
        
        // CORREÇÃO DE PARENT - CRÍTICO (especialmente em builds)
        if (inventory.myHandItem.transform.parent != inventory.pickUpParent)
        {
            Debug.LogWarning($"CORRIGINDO PARENT: Item {inventory.myHandItem.name} não estava parented corretamente");
            
            if (!Application.isEditor)
            {
                // Em builds, usa método mais agressivo
                inventory.myHandItem.transform.SetParent(null);
                inventory.myHandItem.transform.SetParent(inventory.pickUpParent, false);
                ThrowDebugLogger.LogThrow("BUILD: Parent corrigido com método agressivo");
            }
            else
            {
                inventory.myHandItem.transform.SetParent(inventory.pickUpParent);
            }
            
            // Reposiciona para posição local correta
            var weapon = inventory.myHandItem.GetComponent<Weapon>();
            if (weapon != null)
            {
                if (player != null && player.aimActive)
                {
                    inventory.myHandItem.transform.localPosition = weapon.AimOffset;
                    inventory.myHandItem.transform.localRotation = weapon.AimOffsetRotation;
                }
                else
                {
                    inventory.myHandItem.transform.localPosition = weapon.Offset;
                    inventory.myHandItem.transform.localRotation = weapon.OffsetRotation;
                }
            }
        }
        
        var weapon2 = inventory.myHandItem.GetComponent<Weapon>();
        if (weapon2 == null) return;
        
        // Auto-adiciona WeaponOffsetFixer se não existir
        var fixer = inventory.myHandItem.GetComponent<WeaponOffsetFixer>();
        if (fixer == null)
        {
            fixer = inventory.myHandItem.AddComponent<WeaponOffsetFixer>();
            Debug.Log("Auto-adicionado WeaponOffsetFixer à arma");
        }
        
        // Verifica se posição está muito incorreta e corrige
        Vector3 currentPos = inventory.myHandItem.transform.localPosition;
        if (currentPos.y > 2f || currentPos.magnitude > 5f)
        {
            Debug.LogWarning($"Posição da arma muito incorreta: {currentPos}. Aplicando correção automática.");
            fixer.SendMessage("ApplyDefaultOffsets", SendMessageOptions.DontRequireReceiver);
            
            // Força posição correta baseada no estado da mira
            if (player != null && player.aimActive)
            {
                inventory.myHandItem.transform.localPosition = weapon2.AimOffset;
                inventory.myHandItem.transform.localRotation = weapon2.AimOffsetRotation;
            }
            else
            {
                inventory.myHandItem.transform.localPosition = weapon2.Offset;
                inventory.myHandItem.transform.localRotation = weapon2.OffsetRotation;
            }
        }
    }
    
    void ForceBuildParentFix()
    {
        if (inventory?.myHandItem != null && inventory.pickUpParent != null)
        {
            if (inventory.myHandItem.transform.parent != inventory.pickUpParent)
            {
                // Força parent em builds de forma mais agressiva
                inventory.myHandItem.transform.SetParent(inventory.pickUpParent, false);
                
                var weapon = inventory.myHandItem.GetComponent<Weapon>();
                if (weapon != null)
                {
                    inventory.myHandItem.transform.localPosition = weapon.Offset;
                    inventory.myHandItem.transform.localRotation = weapon.OffsetRotation;
                    
                    // Aplica escala correta baseada no tipo
                    ApplyCorrectScale(inventory.myHandItem, weapon);
                }
                
                Debug.Log($"BUILD FIX Frame {buildFixFrameCounter}: Parent forçado");
            }
        }
    }
    
    [ContextMenu("Force Fix Current Weapon")]
    public void ForceFixCurrentWeapon()
    {
        if (inventory?.myHandItem != null)
        {
            // FORÇA CORREÇÃO DE PARENT
            inventory.myHandItem.transform.SetParent(inventory.pickUpParent);
            
            var fixer = inventory.myHandItem.GetComponent<WeaponOffsetFixer>();
            if (fixer == null)
            {
                fixer = inventory.myHandItem.AddComponent<WeaponOffsetFixer>();
            }
            
            fixer.SendMessage("ApplyDefaultOffsets", SendMessageOptions.DontRequireReceiver);
            
            // Força posição
            var weapon = inventory.myHandItem.GetComponent<Weapon>();
            if (weapon != null)
            {
                inventory.myHandItem.transform.localPosition = weapon.Offset;
                inventory.myHandItem.transform.localRotation = weapon.OffsetRotation;
                ApplyCorrectScale(inventory.myHandItem, weapon);
            }
            
            Debug.Log("Weapon forcefully fixed!");
        }
    }
    
    [ContextMenu("Force Parent Fix")]
    public void ForceParentFix()
    {
        if (inventory?.myHandItem != null)
        {
            Debug.Log($"Forçando parent fix - Antes: {inventory.myHandItem.transform.parent?.name ?? "NULL"}");
            inventory.myHandItem.transform.SetParent(inventory.pickUpParent);
            Debug.Log($"Depois: {inventory.myHandItem.transform.parent?.name ?? "NULL"}");
        }
    }
    
    public void ResetBuildFixCounter()
    {
        buildFixFrameCounter = 0;
        Debug.Log("BUILD: Counter resetado para novo item");
    }
    
    void ApplyCorrectScale(GameObject weapon, Weapon weaponComponent)
    {
        Vector3 scale = Vector3.one;
        
        switch (weaponComponent.Type)
        {
            case WeaponType.Pistol:
                scale = new Vector3(1.2f, 1.2f, 1.2f);
                break;
            case WeaponType.Shotgun:
                scale = new Vector3(1.3f, 1.3f, 1.3f);
                break;
            case WeaponType.Target:
                scale = new Vector3(1.25f, 1.25f, 1.25f);
                break;
        }
        
        weapon.transform.localScale = scale;
        Debug.Log($"Escala aplicada {scale} para {weaponComponent.Type}");
    }
    
    [ContextMenu("Increase Weapon Scale")]
    public void IncreaseWeaponScale()
    {
        if (inventory?.myHandItem != null)
        {
            Vector3 currentScale = inventory.myHandItem.transform.localScale;
            Vector3 newScale = currentScale * 1.1f; // Aumenta 10%
            inventory.myHandItem.transform.localScale = newScale;
            Debug.Log($"Escala aumentada para: {newScale}");
        }
    }
    
    [ContextMenu("Decrease Weapon Scale")]
    public void DecreaseWeaponScale()
    {
        if (inventory?.myHandItem != null)
        {
            Vector3 currentScale = inventory.myHandItem.transform.localScale;
            Vector3 newScale = currentScale * 0.9f; // Diminui 10%
            inventory.myHandItem.transform.localScale = newScale;
            Debug.Log($"Escala diminuída para: {newScale}");
        }
    }
}