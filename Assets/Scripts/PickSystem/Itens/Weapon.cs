using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour, IUsable
{
    [field: SerializeField] public UnityEvent OnUse { get; private set; }
    public PlayerInventory inventory;
    public ItemClass itemClass { get; private set; }
    public WeaponType Type { get; set; }
    public int CurrentAmmo;
    public int MaxAmmo;

    private Animator anim;
    public Transform pointBullet;
    private PlayerMoviment player;

    void Start()
    {
        itemClass = GetComponent<ItemClass>();
        anim = GetComponent<Animator>();
        player = FindFirstObjectByType<PlayerMoviment>();

        UpdateInventoryAmmo();
    }

    void Update()
    {
        if (CurrentAmmo < 1)
        {
            anim.SetBool("Ammo", false);
            anim.SetTrigger("NoAmmo");
        }
    }

    public void Use(GameObject actor)
    {
        switch (Type)
        {
            case WeaponType.Pistol:
                UsePistol();
                break;
            case WeaponType.Shotgun:
                UseShotgun();
                break;
            case WeaponType.Target:
                UseTarget();
                break;
        }
    }

    public void UsePistol()
    {
        if (CurrentAmmo <= 0)
        {
            Debug.Log("No ammo left!");
            return;
        }
        CurrentAmmo--;
        UpdateInventoryAmmo();
        OnUse?.Invoke();
    }

    public void UseShotgun()
    {
        if (CurrentAmmo <= 0)
        {
            Debug.Log("No ammo left!");
            return;
        }
        CurrentAmmo--;
        UpdateInventoryAmmo();
        OnUse?.Invoke();
    }

    public void UseTarget()
    {
        OnUse?.Invoke();
    }

    public void ShootProjectile()
    {
        if (CurrentAmmo <= 0) return;

        // Exemplo simples: instanciar projétil
        if (itemClass.projectilePrefab != null)
        {
            // instancia no ponto da arma
            GameObject bullet = Instantiate(
                itemClass.projectilePrefab,
                pointBullet.position,
                pointBullet.rotation
            );

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Calcula a direção do centro da tela (mira).
                Camera cam = Camera.main;
                Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
                Ray ray = cam.ScreenPointToRay(screenCenter);
                Vector3 shootDirection = ray.direction;

                rb.AddForce(shootDirection * (itemClass.projectileSpeed * 3f));
            }
        }

        Debug.Log($"{Type} fired!");
    }

    // public void PlayShootSound()
    // {
    //     if (itemClass.shootSound != null)
    //     {
    //         AudioSource.PlayClipAtPoint(itemClass.shootSound, transform.position);
    //     }
    // }

    public void PlayShootAnimation()
    {
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

        GameObject AmmoSlot = inventory.AmmoSlot;
        TextMeshProUGUI ammoText = AmmoSlot.GetComponentInChildren<TextMeshProUGUI>();
        ammoText.text = CurrentAmmo + " / " + MaxAmmo;

        if (itemInDB != null)
        {
            itemInDB.CurrentAmmo = CurrentAmmo;
        }
    }
}
