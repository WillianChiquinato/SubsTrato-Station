using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/ItemSO")]
public class ItemSO : ScriptableObject
{
    [Header("Item Information")]
    public ItemType itemType;
    public string itemName;
    public Sprite itemIcon;
}

public enum ItemType
{
    Weapon,
    Food,
    Chip
};
