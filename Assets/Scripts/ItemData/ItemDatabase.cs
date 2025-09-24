using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Pistol,
    Shotgun,
    Target,
    BaseItem
}

[CreateAssetMenu(fileName = "NewItemDatabase", menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [System.Serializable]
    public class ItemEntry
    {
        public ItemSO itemSO;
        public GameObject prefab;
        public WeaponType type;
        public int CurrentAmmo;
        public int MaxAmmo;
    }

    public List<ItemEntry> items;

    public static ItemDatabase instance { get; set; }

    public static void LoadInstance(ItemDatabase db)
    {
        instance = db;
    }

    public static GameObject GetPrefabForItem(ItemSO item)
    {
        if (instance == null)
        {
            Debug.LogError("ItemDatabase instance is null! Did you call LoadInstance?");
            return null;
        }

        if (instance.items == null)
        {
            Debug.LogError("ItemDatabase items list is null!");
            return null;
        }

        if (item == null)
        {
            Debug.LogWarning("Item is null in GetPrefabForItem");
            return null;
        }

        foreach (var entry in instance.items)
        {
            if (entry.itemSO == item)
            {
                return entry.prefab;
            }
        }

        return null;
    }

    public static GameObject GetItemEntryById(int id)
    {
        if (instance == null)
        {
            Debug.LogError("ItemDatabase instance is null! Did you call LoadInstance?");
            return null;
        }

        if (instance.items == null)
        {
            Debug.LogError("ItemDatabase items list is null!");
            return null;
        }

        foreach (var entry in instance.items)
        {
            if (entry.itemSO != null && entry.itemSO.id == id)
            {
                return entry.prefab;
            }
        }

        Debug.LogWarning($"Item with ID {id} not found in ItemDatabase.");
        return null;
    }
}
