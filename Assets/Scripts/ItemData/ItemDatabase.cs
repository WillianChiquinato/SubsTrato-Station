using System.Collections.Generic;
using UnityEngine;

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
        foreach (var entry in instance.items)
        {
            if (entry.itemSO == item)
                return entry.prefab;
        }
        return null;
    }
}

