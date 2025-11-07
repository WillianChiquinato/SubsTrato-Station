using UnityEngine;
using System.Collections;

public class BuildWeaponFixer : MonoBehaviour
{
    private PlayerInventory inventory;
    private bool isFixing = false;
    
    void Start()
    {
        inventory = FindFirstObjectByType<PlayerInventory>();
        
        // Só funciona em builds
        if (!Application.isEditor && inventory != null)
        {
            Debug.Log("BuildWeaponFixer ativo - monitorando armas em build");
            InvokeRepeating(nameof(CheckAndFixWeaponParent), 0.1f, 0.1f); // Verifica a cada 0.1s
        }
        else
        {
            // No editor, desativa este componente
            enabled = false;
        }
    }
    
    void CheckAndFixWeaponParent()
    {
        if (inventory?.myHandItem == null || isFixing) return;
        
        if (inventory.myHandItem.transform.parent != inventory.pickUpParent)
        {
            Debug.LogWarning($"BUILD: Detectado problema de parent em {inventory.myHandItem.name}");
            StartCoroutine(FixWeaponParentCoroutine());
        }
    }
    
    IEnumerator FixWeaponParentCoroutine()
    {
        isFixing = true;
        
        if (inventory?.myHandItem != null)
        {
            GameObject item = inventory.myHandItem;
            Transform parentTarget = inventory.pickUpParent;
            
            Debug.Log($"BUILD: Iniciando correção de parent para {item.name}");
            
            item.transform.SetParent(null);
            yield return null;
            
            item.transform.SetParent(parentTarget, false);
            yield return null;
            
            if (item.transform.parent == parentTarget)
            {
                Debug.Log("BUILD: Correção de parent bem-sucedida - Método 1");
                
                var weapon = item.GetComponent<Weapon>();
                if (weapon != null)
                {
                    item.transform.localPosition = weapon.Offset;
                    item.transform.localRotation = weapon.OffsetRotation;
                
                    ApplyWeaponScale(item, weapon);
                }
            }
            else
            {
                Debug.LogWarning("BUILD: Método 1 falhou, tentando força bruta");
                
                for (int i = 0; i < 3; i++)
                {
                    item.transform.SetParent(parentTarget, false);
                    yield return null;
                }
                
                // Última verificação
                if (item.transform.parent == parentTarget)
                {
                    Debug.Log("BUILD: Correção de parent bem-sucedida - Método 2");
                }
                else
                {
                    Debug.LogError("BUILD: FALHA TOTAL na correção de parent!");
                }
            }
        }
        
        isFixing = false;
    }
    
    void ApplyWeaponScale(GameObject weapon, Weapon weaponComponent)
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
        Debug.Log($"BUILD: Escala aplicada {scale} para {weaponComponent.Type}");
    }
    
    // Método para forçar correção manualmente
    [ContextMenu("Force Fix Now")]
    public void ForceFixNow()
    {
        if (!isFixing)
        {
            StartCoroutine(FixWeaponParentCoroutine());
        }
    }
}