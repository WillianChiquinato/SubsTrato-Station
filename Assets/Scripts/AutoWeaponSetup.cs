using UnityEngine;

[System.Serializable]
public class AutoWeaponSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool enableDiagnostic = true;
    public bool enableWeaponFixer = true;
    
    void Start()
    {
        SetupDiagnostics();
    }
    
    void SetupDiagnostics()
    {
        if (enableDiagnostic)
        {
            WeaponDiagnostic diagnostic = FindFirstObjectByType<WeaponDiagnostic>();
            if (diagnostic == null)
            {
                GameObject diagObj = new GameObject("WeaponDiagnostic");
                diagnostic = diagObj.AddComponent<WeaponDiagnostic>();
                ThrowDebugLogger.LogThrow("WeaponDiagnostic adicionado automaticamente");
            }
        }
        
        ThrowDebugLogger.LogThrow("AutoWeaponSetup configurado");
    }
    
    public void ForceFixAllWeapons()
    {
        WeaponOffsetFixer[] fixers = FindObjectsOfType<WeaponOffsetFixer>();
        foreach (var fixer in fixers)
        {
            fixer.SendMessage("ApplyDefaultOffsets", SendMessageOptions.DontRequireReceiver);
        }
        
        // Também adiciona fixers em armas que não têm
        Weapon[] weapons = FindObjectsOfType<Weapon>();
        foreach (var weapon in weapons)
        {
            if (weapon.GetComponent<WeaponOffsetFixer>() == null)
            {
                var fixer = weapon.gameObject.AddComponent<WeaponOffsetFixer>();
                fixer.SendMessage("ApplyDefaultOffsets", SendMessageOptions.DontRequireReceiver);
            }
        }
        
        ThrowDebugLogger.LogThrow($"Forçado fix em {weapons.Length} armas");
    }
}