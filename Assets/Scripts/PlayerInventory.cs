using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
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

    void Awake()
    {
        ItemDatabase.LoadInstance(itemDatabase);
        player = GetComponent<PlayerMoviment>();
        AmmoSlot.SetActive(false);
    }

    void Start()
    {
        UpdateHotbarUI();
        slotHighlight.gameObject.SetActive(false);
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (player.aimAnimActive)
        {
            return;
        }

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
        if (selectedSlot < 0f)
        {
            slotHighlight.gameObject.SetActive(false);
        }
        else
        {
            slotHighlight.position = slotPositions[selectedSlot].position;
            slotHighlight.gameObject.SetActive(true);
        }

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


    void EquipSelectedItem()
    {
        // Destroi o item antigo da mão, se houver
        if (myHandItem != null)
        {
            Destroy(myHandItem);
        }

        ItemSO selectedItem = hotbarItems[selectedSlot];

        if (selectedItem != null)
        {
            GameObject prefab = ItemDatabase.GetPrefabForItem(selectedItem);
            if (prefab != null)
            {
                GameObject newItem = Instantiate(prefab, pickUpParent);
                Rigidbody rb = newItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Destroy(rb);
                }
                newItem.transform.localPosition = Vector3.zero;
                newItem.transform.localRotation = Quaternion.identity;

                myHandItem = newItem;

                if (myHandItem.TryGetComponent<Weapon>(out var gun))
                {
                    var itemInDB = itemDatabase.items.FirstOrDefault(x => x.itemSO == selectedItem);
                    if (itemInDB != null)
                    {
                        gun.inventory = this;
                        gun.Type = itemInDB.type;
                        gun.MaxAmmo = itemInDB.MaxAmmo;
                        gun.CurrentAmmo = itemInDB.CurrentAmmo;

                        AmmoSlot.SetActive(true);
                        AmmoSlot.GetComponentInChildren<TextMeshProUGUI>()
                            .text = gun.CurrentAmmo + " / " + gun.MaxAmmo;
                    }
                }

                Debug.Log("Equipado item na mão: " + selectedItem.itemName);
            }
            else
            {
                Debug.LogWarning("Prefab não encontrado para item: " + selectedItem.name);
            }
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
