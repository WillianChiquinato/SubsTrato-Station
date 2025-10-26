using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    public NetworkObject networkObject;

    public ItemDatabase itemDatabase;
    public PlayerMoviment player;

    public int totalSlots = 6;
    public int selectedSlot = 0;

    [Header("Hotbar Slots")]
    public RectTransform slotHighlight;
    public RectTransform[] slotPositions;
    public Image[] slotIcons;

    [Header("Itens na hotbar")]
    public ItemSO[] hotbarItems;
    public Transform pickUpParent;
    public GameObject myHandItem;
    public bool justDroppedItem = false;

    [Header("UI")]
    public GameObject AmmoSlot;

    void Start()
    {
        ItemDatabase.LoadInstance(itemDatabase);
        player = GetComponent<PlayerMoviment>();
        networkObject = GetComponent<NetworkObject>();

        //References UI.
        var ui = UIreferences.Instance;
        slotHighlight = ui.SlotHighlight;
        slotPositions = ui.slotsPositions;
        slotIcons = ui.slotIcons;
        AmmoSlot = ui.AmmoSlot;
        AmmoSlot.SetActive(false);
        slotHighlight.gameObject.SetActive(false);

        IconeUpdate();
    }

    void Update()
    {
        if (!player.networkObject.HasInputAuthority) return;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (player.aimAnimActive) return;
        if (player.Arremessar) return;

        if (scroll > 0f)
        {
            selectedSlot = (selectedSlot + 1) % totalSlots;
            UpdateHotbarUI();
        }
        else if (scroll < 0f)
        {
            selectedSlot = (selectedSlot - 1 + totalSlots) % totalSlots;
            UpdateHotbarUI();
        }

        for (int i = 0; i < totalSlots; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedSlot = i;
                UpdateHotbarUI();
            }
        }
    }

    public void UpdateHotbarUI()
    {
        if (!player.networkObject.HasInputAuthority) return;

        if (selectedSlot < 0f)
        {
            slotHighlight.gameObject.SetActive(false);
        }
        else
        {
            slotHighlight.position = slotPositions[selectedSlot].position;
            slotHighlight.gameObject.SetActive(true);
        }

        IconeUpdate();

        if (!justDroppedItem)
        {
            EquipSelectedItem();

        }
        else
        {
            justDroppedItem = false;
        }

        Debug.Log("Item selecionado: " + hotbarItems[selectedSlot]?.itemName);
    }

    public void IconeUpdate()
    {
        for (int i = 0; i < totalSlots; i++)
        {
            if (hotbarItems[i] != null)
            {
                slotIcons[i].sprite = hotbarItems[i].itemIcon;
                slotIcons[i].enabled = true;
            }
            else
            {
                slotIcons[i].enabled = false;
            }
        }
    }


    void EquipSelectedItem()
    {
        // Destroi o item antigo da mão, se houver
        if (myHandItem != null)
        {
            if (myHandItem.TryGetComponent<NetworkObject>(out var netObj))
            {
                networkObject.Runner.Despawn(netObj);
            }
            else
            {
                Destroy(myHandItem);
            }
        }

        ItemSO selectedItem = hotbarItems[selectedSlot];

        if (selectedItem != null)
        {
            GameObject prefab = ItemDatabase.GetPrefabForItem(selectedItem);
            if (prefab != null)
            {
                if (player.networkObject.HasStateAuthority)
                {
                    NetworkObject newItemNet = networkObject.Runner.Spawn(prefab, pickUpParent.position, pickUpParent.rotation, player.networkObject.InputAuthority);
                    newItemNet.transform.SetParent(pickUpParent);
                    Rigidbody rb = newItemNet.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Destroy(rb);
                    }

                    if (newItemNet.TryGetComponent<Weapon>(out var weapon))
                    {
                        newItemNet.transform.localPosition += weapon.Offset;
                        newItemNet.transform.localRotation = weapon.OffsetRotation;
                    }

                    myHandItem = newItemNet.gameObject;

                    if (myHandItem.TryGetComponent<Weapon>(out var gun))
                    {
                        var itemInDB = itemDatabase.items.FirstOrDefault(x => x.itemSO == selectedItem);
                        if (itemInDB != null)
                        {
                            gun.inventory = this;
                            gun.Type = itemInDB.type;
                            gun.MaxAmmo = itemInDB.MaxAmmo;
                            gun.CurrentAmmo = itemInDB.CurrentAmmo;

                            if (!gun.GetComponent<ItemArremessavel>())
                            {
                                AmmoSlot.SetActive(true);
                                AmmoSlot.GetComponentInChildren<TextMeshProUGUI>()
                                    .text = gun.CurrentAmmo + " / " + gun.MaxAmmo;
                            }
                            else
                            {
                                AmmoSlot.SetActive(true);
                                AmmoSlot.GetComponentInChildren<TextMeshProUGUI>()
                                    .text = "<Arremessar>";
                            }
                        }
                    }

                    Debug.Log("Equipado item na mão: " + selectedItem.itemName);
                }
            }
            else
            {
                Debug.LogWarning("Prefab não encontrado para item: " + selectedItem.name);
            }
        }
        else
        {
            AmmoSlot.SetActive(false);
        }
    }

    public bool AddItemToHotbar(ItemSO newItem)
    {
        for (int i = 0; i < hotbarItems.Length; i++)
        {
            if (hotbarItems[i] == null)
            {
                hotbarItems[i] = newItem;
                UpdateHotbarUI();
                return true;
            }
        }

        Debug.Log("Hotbar cheia! Não foi possível adicionar: " + newItem.itemName);
        return false;
    }
}
