using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDatabase", menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [System.Serializable]
    public class ItemEntry
    {
        public ItemSO itemSO;
        public GameObject prefab;
    }

    public List<ItemEntry> items;

    private static ItemDatabase instance;

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
}
