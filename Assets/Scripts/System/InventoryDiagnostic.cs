using Fusion;
using UnityEngine;

public class InventoryDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    public bool enableAutoCheck = true;
    public float checkInterval = 5f;
    
    private PlayerInventory inventory;
    private PlayerMoviment player;
    private float lastCheckTime;
    
    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        player = GetComponent<PlayerMoviment>();
        
        if (inventory == null)
        {
            ThrowDebugLogger.LogThrowError("PlayerInventory não encontrado");
        }
        
        if (player == null)
        {
            ThrowDebugLogger.LogThrowError("PlayerMoviment não encontrado");
        }
    }
    
    void Update()
    {
        if (enableAutoCheck && Time.time - lastCheckTime > checkInterval)
        {
            PerformDiagnostic();
            lastCheckTime = Time.time;
        }
        
        // Atalho para diagnóstico manual
        if (Input.GetKeyDown(KeyCode.F11))
        {
            PerformFullDiagnostic();
        }
        
        // Detecta especificamente o problema do botão direito
        if (Input.GetMouseButtonDown(1))
        {
            CheckRightClickIssue();
        }
    }
    
    void CheckRightClickIssue()
    {
        if (inventory == null || player == null) return;
        
        ThrowDebugLogger.LogThrow("=== RIGHT CLICK ISSUE CHECK ===");
        
        // Estado antes do clique
        bool hadItemBefore = inventory.myHandItem != null;
        string itemNameBefore = hadItemBefore ? inventory.myHandItem.name : "NONE";
        bool isThrowable = hadItemBefore && inventory.myHandItem.GetComponent<ItemArremessavel>() != null;
        bool isWeapon = hadItemBefore && inventory.myHandItem.GetComponent<Weapon>() != null;
        
        ThrowDebugLogger.LogThrow($"ANTES do RMB - Item: {itemNameBefore}, Arremessável: {isThrowable}, Arma: {isWeapon}");
        ThrowDebugLogger.LogThrow($"Player aimActive: {player.aimActive}, aimAnimActive: {player.aimAnimActive}");
        ThrowDebugLogger.LogThrow($"Player Arremessar: {player.Arremessar}");
        
        // Agenda verificação após o processamento
        Invoke(nameof(CheckAfterRightClick), 0.1f);
    }
    
    void CheckAfterRightClick()
    {
        if (inventory == null) return;
        
        bool hasItemAfter = inventory.myHandItem != null;
        string itemNameAfter = hasItemAfter ? inventory.myHandItem.name : "NONE";
        
        ThrowDebugLogger.LogThrow($"DEPOIS do RMB - Item: {itemNameAfter}");
        
        if (!hasItemAfter)
        {
            ThrowDebugLogger.LogThrowError("PROBLEMA DETECTADO: Arma sumiu após clicar RMB!");
            ThrowDebugLogger.LogThrow($"Slot selecionado: {inventory.selectedSlot}");
            ThrowDebugLogger.LogThrow($"Item no slot: {(inventory.hotbarItems[inventory.selectedSlot]?.itemName ?? "EMPTY")}");
        }
        
        ThrowDebugLogger.LogThrow("=== END RIGHT CLICK CHECK ===");
    }
    
    [ContextMenu("Perform Full Diagnostic")]
    public void PerformFullDiagnostic()
    {
        ThrowDebugLogger.LogThrow("=== INVENTORY FULL DIAGNOSTIC ===");
        
        if (inventory == null || player == null)
        {
            ThrowDebugLogger.LogThrowError("Componentes necessários não encontrados");
            return;
        }
        
        // Estado do jogador
        ThrowDebugLogger.LogThrow($"Player HasInputAuthority: {(player.networkObject != null ? player.networkObject.HasInputAuthority.ToString() : "NULL")}");
        ThrowDebugLogger.LogThrow($"Player HasStateAuthority: {(player.networkObject != null ? player.networkObject.HasStateAuthority.ToString() : "NULL")}");
        ThrowDebugLogger.LogThrow($"Player aimActive: {player.aimActive}");
        ThrowDebugLogger.LogThrow($"Player aimAnimActive: {player.aimAnimActive}");
        ThrowDebugLogger.LogThrow($"Player Arremessar: {player.Arremessar}");
        
        // Estado do inventário
        ThrowDebugLogger.LogThrow($"Selected Slot: {inventory.selectedSlot}");
        ThrowDebugLogger.LogThrow($"Total Slots: {inventory.totalSlots}");
        ThrowDebugLogger.LogThrow($"MyHandItem: {(inventory.myHandItem != null ? inventory.myHandItem.name : "NULL")}");
        
        // Itens na hotbar
        for (int i = 0; i < inventory.hotbarItems.Length; i++)
        {
            string itemName = inventory.hotbarItems[i] != null ? inventory.hotbarItems[i].itemName : "EMPTY";
            ThrowDebugLogger.LogThrow($"Slot {i}: {itemName}");
        }
        
        // Estado do item na mão
        if (inventory.myHandItem != null)
        {
            var weapon = inventory.myHandItem.GetComponent<Weapon>();
            var networkObj = inventory.myHandItem.GetComponent<NetworkObject>();
            var itemClass = inventory.myHandItem.GetComponent<ItemClass>();
            
            ThrowDebugLogger.LogThrow($"HandItem Position: {inventory.myHandItem.transform.position}");
            ThrowDebugLogger.LogThrow($"HandItem Local Position: {inventory.myHandItem.transform.localPosition}");
            ThrowDebugLogger.LogThrow($"HandItem Active: {inventory.myHandItem.activeInHierarchy}");
            ThrowDebugLogger.LogThrow($"Has Weapon Component: {weapon != null}");
            ThrowDebugLogger.LogThrow($"Has NetworkObject: {networkObj != null}");
            ThrowDebugLogger.LogThrow($"Has ItemClass: {itemClass != null}");
            
            if (weapon != null)
            {
                ThrowDebugLogger.LogThrow($"Weapon Type: {weapon.Type}");
                ThrowDebugLogger.LogThrow($"Weapon CurrentAmmo: {weapon.CurrentAmmo}");
                ThrowDebugLogger.LogThrow($"Weapon MaxAmmo: {weapon.MaxAmmo}");
                ThrowDebugLogger.LogThrow($"Weapon Offset: {weapon.Offset}");
                ThrowDebugLogger.LogThrow($"Weapon AimOffset: {weapon.AimOffset}");
            }
        }
        
        // Estado da rede
        if (inventory.networkObject != null)
        {
            ThrowDebugLogger.LogThrow($"Inventory NetworkObject IsValid: {inventory.networkObject.IsValid}");
            ThrowDebugLogger.LogThrow($"Inventory Runner: {(inventory.networkObject.Runner != null ? "EXISTS" : "NULL")}");
        }
        
        ThrowDebugLogger.LogThrow("=== END INVENTORY DIAGNOSTIC ===");
    }
    
    void PerformDiagnostic()
    {
        if (inventory == null) return;
        
        // Diagnóstico básico automático
        string handItemStatus = inventory.myHandItem != null ? inventory.myHandItem.name : "EMPTY";
        string selectedItem = inventory.hotbarItems[inventory.selectedSlot] != null ? 
                             inventory.hotbarItems[inventory.selectedSlot].itemName : "EMPTY";
        
        ThrowDebugLogger.LogThrow($"[AUTO_CHECK] Slot {inventory.selectedSlot}: {selectedItem} | Hand: {handItemStatus}");
        
        // Verifica problemas comuns
        if (inventory.hotbarItems[inventory.selectedSlot] != null && inventory.myHandItem == null)
        {
            ThrowDebugLogger.LogThrowWarning("PROBLEMA: Item no slot mas não na mão!");
        }
        
        if (inventory.myHandItem != null && !inventory.myHandItem.activeInHierarchy)
        {
            ThrowDebugLogger.LogThrowWarning("PROBLEMA: Item na mão mas inativo!");
        }
    }
    
    [ContextMenu("Force Equip Current Slot")]
    public void ForceEquipCurrentSlot()
    {
        if (inventory != null)
        {
            ThrowDebugLogger.LogThrow("Forçando equipar item do slot atual");
            inventory.UpdateHotbarUI();
        }
    }
    
    [ContextMenu("Clear Hand Item")]
    public void ClearHandItem()
    {
        if (inventory != null && inventory.myHandItem != null)
        {
            ThrowDebugLogger.LogThrow("Limpando item da mão manualmente");
            Destroy(inventory.myHandItem);
            inventory.myHandItem = null;
        }
    }
}