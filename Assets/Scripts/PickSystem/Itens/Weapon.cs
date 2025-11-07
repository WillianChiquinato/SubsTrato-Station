using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour, IUsable
{
    [Header("Sounds")]
    public AudioSource shootSound;

    [Header("Shooting Settings")]
    private float lastShootTime = -999f;
    [SerializeField] private float shootCooldown = 0.7f;
    [field: SerializeField] public UnityEvent OnUse { get; private set; }
    [HideInInspector] public PlayerInventory inventory;
    public ItemClass itemClass { get; private set; }
    public WeaponType Type { get; set; }
    public int CurrentAmmo;
    public int MaxAmmo;

    private Animator anim;
    public Transform pointBullet;
    private PlayerMoviment player;
    public Vector3 Offset;
    public Quaternion OffsetRotation;
    public Vector3 AimOffset;
    public Quaternion AimOffsetRotation;

    void Start()
    {
        itemClass = GetComponent<ItemClass>();
        if (WeaponType.Target != Type)
        {
            anim = GetComponent<Animator>();
        }
        
        player = FindFirstObjectByType<PlayerMoviment>();
        
        // Debug para verificar inicialização
        ThrowDebugLogger.LogThrow($"Weapon iniciada - Tipo: {Type}, CurrentAmmo: {CurrentAmmo}, MaxAmmo: {MaxAmmo}");
        
        if (itemClass == null)
        {
            ThrowDebugLogger.LogThrowWarning("ItemClass não encontrado na arma");
        }
        
        if (player == null)
        {
            ThrowDebugLogger.LogThrowWarning("PlayerMoviment não encontrado");
        }
    }

    void Update()
    {
        if (WeaponType.Target != Type)
        {
            if (CurrentAmmo <= 0 && anim != null)
            {
                anim.SetBool("Ammo", false);
                anim.SetTrigger("NoAmmo");
            }
        }

        // Log apenas ocasionalmente para não spammar
        if (Time.frameCount % 300 == 0)
        {
            ThrowDebugLogger.LogThrow($"Weapon Update - Tipo: {Type}, Munição: {CurrentAmmo}/{MaxAmmo}");
        }
    }

    public void Use(GameObject actor)
    {
        ThrowDebugLogger.LogThrow($"Weapon.Use chamado - Tipo: {Type}, Cooldown restante: {shootCooldown - (Time.time - lastShootTime)}");
        
        if (Time.time - lastShootTime < shootCooldown)
        {
            ThrowDebugLogger.LogThrow("Uso bloqueado por cooldown");
            return;
        }

        switch (Type)
        {
            case WeaponType.Pistol:
                ThrowDebugLogger.LogThrow("Usando pistola");
                UsePistol();
                break;
            case WeaponType.Shotgun:
                ThrowDebugLogger.LogThrow("Usando shotgun");
                UseShotgun();
                break;
            case WeaponType.Target:
                ThrowDebugLogger.LogThrow("Usando arma de alvo");
                UseTarget();
                break;
            default:
                ThrowDebugLogger.LogThrowWarning($"Tipo de arma desconhecido: {Type}");
                break;
        }
        lastShootTime = Time.time;
    }

    public void UsePistol()
    {
        if (CurrentAmmo <= 0)
        {
            ThrowDebugLogger.LogThrow("Pistola sem munição");
            Debug.Log("No ammo left!");
            return;
        }
        
        CurrentAmmo--;
        ThrowDebugLogger.LogThrow($"Pistola disparada - Munição restante: {CurrentAmmo}");
        UpdateInventoryAmmo();
        OnUse?.Invoke();
    }

    public void UseShotgun()
    {
        if (CurrentAmmo <= 0)
        {
            ThrowDebugLogger.LogThrow("Shotgun sem munição");
            Debug.Log("No ammo left!");
            return;
        }
        
        CurrentAmmo--;
        ThrowDebugLogger.LogThrow($"Shotgun disparada - Munição restante: {CurrentAmmo}");
        UpdateInventoryAmmo();
        OnUse?.Invoke();
    }

    public void UseTarget()
    {
        ThrowDebugLogger.LogThrow("Arma de alvo usada");
        OnUse?.Invoke();
        Debug.Log("Target weapon used");
    }

    public void ShootProjectile()
    {
        if (CurrentAmmo < 0) return;

        if (itemClass.projectilePrefab != null)
        {
            GameObject bullet = Instantiate(
                itemClass.projectilePrefab,
                pointBullet.position,
                pointBullet.rotation
            );

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Camera cam = Camera.main;
                Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
                Ray ray = cam.ScreenPointToRay(screenCenter);
                Vector3 shootDirection = ray.direction;

                rb.AddForce(shootDirection * (itemClass.projectileSpeed * 3f));
            }
        }

        Debug.Log($"{Type} fired!");
    }

    public void PlayShootSound()
    {
        if (CurrentAmmo < 0) return;

        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound.clip, transform.position);
        }
    }

    public void PlayShootAnimation()
    {
        if (CurrentAmmo < 0) return;

        if (anim != null)
        {
            if (CurrentAmmo < 1)
            {
                anim.SetBool("Ammo", false);
                anim.SetTrigger("NoAmmo");
            }
            else
            {
                anim.SetBool("Ammo", true);
                anim.SetTrigger("Shoot");
            }
        }
    }

    private void UpdateInventoryAmmo()
    {
        var itemInDB = inventory.itemDatabase.items
            .FirstOrDefault(x => x.itemSO == itemClass.itemSO);

        Type = itemInDB.type;

        GameObject AmmoSlot = inventory.AmmoSlot;
        TextMeshProUGUI ammoText = AmmoSlot.GetComponentInChildren<TextMeshProUGUI>();
        ammoText.text = CurrentAmmo + " / " + MaxAmmo;

        if (itemInDB != null)
        {
            itemInDB.CurrentAmmo = CurrentAmmo;
        }
    }
}
