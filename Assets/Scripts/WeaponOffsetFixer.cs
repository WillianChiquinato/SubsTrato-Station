using UnityEngine;

public class WeaponOffsetFixer : MonoBehaviour
{
    [Header("Configurações Padrão de Armas")]
    [SerializeField] private Vector3 defaultPistolOffset = new Vector3(0.3f, -0.1f, 0.5f);
    [SerializeField] private Vector3 defaultShotgunOffset = new Vector3(0.2f, -0.15f, 0.6f);
    [SerializeField] private Vector3 defaultTargetOffset = new Vector3(0.25f, -0.12f, 0.55f);
    
    [SerializeField] private Quaternion defaultPistolRotation = Quaternion.Euler(0, 0, 0);
    [SerializeField] private Quaternion defaultShotgunRotation = Quaternion.Euler(0, 0, 0);
    [SerializeField] private Quaternion defaultTargetRotation = Quaternion.Euler(0, 0, 0);
    
    [Header("Escalas das Armas")]
    [SerializeField] private Vector3 defaultPistolScale = new Vector3(2.5f, 2.5f, 2.5f);
    [SerializeField] private Vector3 defaultShotgunScale = new Vector3(1.7f, 1.7f, 1.7f);
    [SerializeField] private Vector3 defaultTargetScale = new Vector3(1.25f, 1.25f, 1.25f);
    
    [Header("Mira - Offsets")]
    [SerializeField] private Vector3 defaultPistolAimOffset = new Vector3(0f, 0.05f, 0.3f);
    [SerializeField] private Vector3 defaultShotgunAimOffset = new Vector3(0f, 0.02f, 0.4f);
    [SerializeField] private Vector3 defaultTargetAimOffset = new Vector3(0f, 0.03f, 0.35f);
    
    [SerializeField] private Quaternion defaultPistolAimRotation = Quaternion.Euler(0, 0, 0);
    [SerializeField] private Quaternion defaultShotgunAimRotation = Quaternion.Euler(0, 0, 0);
    [SerializeField] private Quaternion defaultTargetAimRotation = Quaternion.Euler(0, 0, 0);

    void Start()
    {
        ApplyDefaultOffsets();
    }

    void ApplyDefaultOffsets()
    {
        Weapon weapon = GetComponent<Weapon>();
        if (weapon == null) return;

        bool offsetsNeedFix = false;
        Vector3 currentOffset = weapon.Offset;
        
        // Verifica se os offsets estão incorretos (muito altos ou extremos)
        if (currentOffset.y > 1f || currentOffset.magnitude > 2f || currentOffset == Vector3.zero)
        {
            offsetsNeedFix = true;
            ThrowDebugLogger.LogThrow($"Offsets incorretos detectados na arma {gameObject.name}: {currentOffset}");
        }

        if (offsetsNeedFix)
        {
            switch (weapon.Type)
            {
                case WeaponType.Pistol:
                    weapon.Offset = defaultPistolOffset;
                    weapon.OffsetRotation = defaultPistolRotation;
                    weapon.AimOffset = defaultPistolAimOffset;
                    weapon.AimOffsetRotation = defaultPistolAimRotation;
                    transform.localScale = defaultPistolScale;
                    ThrowDebugLogger.LogThrow($"Pistola corrigida - Novo offset: {defaultPistolOffset}, Escala: {defaultPistolScale}");
                    break;
                    
                case WeaponType.Shotgun:
                    weapon.Offset = defaultShotgunOffset;
                    weapon.OffsetRotation = defaultShotgunRotation;
                    weapon.AimOffset = defaultShotgunAimOffset;
                    weapon.AimOffsetRotation = defaultShotgunAimRotation;
                    transform.localScale = defaultShotgunScale;
                    ThrowDebugLogger.LogThrow($"Shotgun corrigida - Novo offset: {defaultShotgunOffset}, Escala: {defaultShotgunScale}");
                    break;
                    
                case WeaponType.Target:
                    weapon.Offset = defaultTargetOffset;
                    weapon.OffsetRotation = defaultTargetRotation;
                    weapon.AimOffset = defaultTargetAimOffset;
                    weapon.AimOffsetRotation = defaultTargetAimRotation;
                    transform.localScale = defaultTargetScale;
                    ThrowDebugLogger.LogThrow($"Arma Target corrigida - Novo offset: {defaultTargetOffset}, Escala: {defaultTargetScale}");
                    break;
            }
        }
        else
        {
            // Mesmo se os offsets estão corretos, aplica a escala padrão
            switch (weapon.Type)
            {
                case WeaponType.Pistol:
                    transform.localScale = defaultPistolScale;
                    break;
                case WeaponType.Shotgun:
                    transform.localScale = defaultShotgunScale;
                    break;
                case WeaponType.Target:
                    transform.localScale = defaultTargetScale;
                    break;
            }
            ThrowDebugLogger.LogThrow($"Offsets da arma {gameObject.name} estão corretos: {currentOffset}, Escala aplicada");
        }
    }

    [ContextMenu("Fix All Weapon Offsets")]
    public void FixAllWeaponOffsets()
    {
        Weapon[] allWeapons = FindObjectsOfType<Weapon>();
        foreach (var weapon in allWeapons)
        {
            var fixer = weapon.gameObject.GetComponent<WeaponOffsetFixer>();
            if (fixer == null)
            {
                fixer = weapon.gameObject.AddComponent<WeaponOffsetFixer>();
            }
            fixer.ApplyDefaultOffsets();
        }
        
        ThrowDebugLogger.LogThrow($"Corrigidos offsets de {allWeapons.Length} armas");
    }

    [ContextMenu("Log Current Weapon Offsets")]
    public void LogCurrentOffsets()
    {
        Weapon weapon = GetComponent<Weapon>();
        if (weapon != null)
        {
            ThrowDebugLogger.LogThrow($"=== OFFSETS DA ARMA {gameObject.name} ===");
            ThrowDebugLogger.LogThrow($"Tipo: {weapon.Type}");
            ThrowDebugLogger.LogThrow($"Offset Normal: {weapon.Offset}");
            ThrowDebugLogger.LogThrow($"Rotation Normal: {weapon.OffsetRotation}");
            ThrowDebugLogger.LogThrow($"Offset Mira: {weapon.AimOffset}");
            ThrowDebugLogger.LogThrow($"Rotation Mira: {weapon.AimOffsetRotation}");
            ThrowDebugLogger.LogThrow($"Posição Atual: {transform.localPosition}");
            ThrowDebugLogger.LogThrow($"================================");
        }
    }
}