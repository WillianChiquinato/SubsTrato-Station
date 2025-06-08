using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    public ItemDatabase itemDatabase;

    public int totalSlots = 6;
    public int selectedSlot = 0;

    [Header("Hotbar Slots")]
    public Image[] slotHighlights; // Borda de seleção
    public Image[] slotIcons;

    [Header("Itens na hotbar")]
    public ItemSO[] hotbarItems;
    public Transform pickUpParent;
    public GameObject myHandItem;

    void Awake()
    {
        ItemDatabase.LoadInstance(itemDatabase);
    }

    void Start()
    {
        UpdateHotbarUI();
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

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
        for (int i = 0; i < totalSlots; i++)
        {
            slotHighlights[i].enabled = (i == selectedSlot);

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

        EquipSelectedItem();

        Debug.Log("Item selecionado: " + hotbarItems[selectedSlot]?.itemName);
    }

    void EquipSelectedItem()
    {
        // Destroi o item antigo da mão, se houver
        if (myHandItem != null)
        {
            Destroy(myHandItem);
        }

        // Pega o item selecionado na hotbar
        ItemSO selectedItem = hotbarItems[selectedSlot];

        if (selectedItem != null)
        {
            // Cria uma instância do prefab (você precisará mapear qual prefab pertence ao ScriptableObject)
            GameObject prefab = ItemDatabase.GetPrefabForItem(selectedItem); // você criará isso
            if (prefab != null)
            {
                GameObject newItem = Instantiate(prefab, pickUpParent);
                newItem.transform.localPosition = Vector3.zero;
                newItem.transform.localRotation = Quaternion.identity;

                myHandItem = newItem;
            }
        }
    }

}
