using System.Linq;
using System.Collections;
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
        
        if (player.aimAnimActive) 
        {
            ThrowDebugLogger.LogThrow("Mudança de slot bloqueada - mira ativa");
            return;
        }
        
        if (player.Arremessar) 
        {
            ThrowDebugLogger.LogThrow("Mudança de slot bloqueada - arremesso ativo");
            return;
        }

        bool slotChanged = false;

        if (scroll > 0f)
        {
            int newSlot = (selectedSlot + 1) % totalSlots;
            if (newSlot != selectedSlot)
            {
                selectedSlot = newSlot;
                slotChanged = true;
                ThrowDebugLogger.LogThrow($"Slot mudado via scroll UP para: {selectedSlot}");
            }
        }
        else if (scroll < 0f)
        {
            int newSlot = (selectedSlot - 1 + totalSlots) % totalSlots;
            if (newSlot != selectedSlot)
            {
                selectedSlot = newSlot;
                slotChanged = true;
                ThrowDebugLogger.LogThrow($"Slot mudado via scroll DOWN para: {selectedSlot}");
            }
        }

        for (int i = 0; i < totalSlots; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                if (selectedSlot != i)
                {
                    selectedSlot = i;
                    slotChanged = true;
                    ThrowDebugLogger.LogThrow($"Slot mudado via tecla {i + 1} para: {selectedSlot}");
                }
                break;
            }
        }

        if (slotChanged)
        {
            UpdateHotbarUI();
        }
    }

    public void UpdateHotbarUI()
    {
        if (!player.networkObject.HasInputAuthority) return;

        ThrowDebugLogger.LogThrow($"UpdateHotbarUI chamado - Slot: {selectedSlot}");

        if (selectedSlot < 0f)
        {
            slotHighlight.gameObject.SetActive(false);
            ThrowDebugLogger.LogThrow("Slot highlight desativado - selectedSlot < 0");
        }
        else
        {
            slotHighlight.position = slotPositions[selectedSlot].position;
            slotHighlight.gameObject.SetActive(true);
        }

        IconeUpdate();

        if (!justDroppedItem)
        {
            ThrowDebugLogger.LogThrow("Equipando item selecionado");
            EquipSelectedItem();
        }
        else
        {
            ThrowDebugLogger.LogThrow("Pulando equip - justDroppedItem = true");
            justDroppedItem = false;
        }

        string itemName = hotbarItems[selectedSlot]?.itemName ?? "EMPTY";
        ThrowDebugLogger.LogThrow($"Item selecionado: {itemName}");
        Debug.Log("Item pego com sucesso!");
    }

    // Coroutine específica para garantir parenting correto em builds
    private IEnumerator EnsureParentingInBuild(GameObject newItem, ItemSO selectedItem)
    {
        ThrowDebugLogger.LogThrow("BUILD: Iniciando EnsureParentingInBuild");
        yield return null;
        
        if (newItem != null)
        {
            ThrowDebugLogger.LogThrow($"BUILD: Reforçando parent - Atual: {(newItem.transform.parent != null ? newItem.transform.parent.name : "NULL")}");
            
            newItem.transform.SetParent(pickUpParent, false);
            
            if (newItem.transform.parent == pickUpParent)
            {
                ThrowDebugLogger.LogThrow("BUILD: Parent aplicado com sucesso");
                ConfigureSpawnedItem(newItem, selectedItem);
            }
            else
            {
                ThrowDebugLogger.LogThrowError("BUILD: FALHA ao aplicar parent - tentando força bruta");
                
                newItem.transform.SetParent(null);
                yield return null;
                newItem.transform.SetParent(pickUpParent, false);
                
                ConfigureSpawnedItem(newItem, selectedItem);
            }
        }
        else
        {
            ThrowDebugLogger.LogThrowError("BUILD: newItem foi destruído durante coroutine");
        }
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
        ItemSO selectedItem = hotbarItems[selectedSlot];
        
        ThrowDebugLogger.LogThrow($"=== EQUIP ITEM DEBUG ===");
        ThrowDebugLogger.LogThrow($"SelectedSlot: {selectedSlot}");
        ThrowDebugLogger.LogThrow($"SelectedItem: {(selectedItem != null ? selectedItem.itemName : "NULL")}");
        ThrowDebugLogger.LogThrow($"CurrentHandItem: {(myHandItem != null ? myHandItem.name : "NULL")}");
        
        if (selectedItem != null && myHandItem != null)
        {
            var currentItemClass = myHandItem.GetComponent<ItemClass>();
            if (currentItemClass != null && currentItemClass.itemSO == selectedItem)
            {
                ThrowDebugLogger.LogThrow("Item correto já está na mão - pulando equipar");
                return;
            }
        }
        
        if (selectedItem == null && myHandItem == null)
        {
            ThrowDebugLogger.LogThrow("Nenhum item para equipar - pulando");
            AmmoSlot.SetActive(false);
            return;
        }
        
        ThrowDebugLogger.LogThrow($"HasStateAuthority: {(player.networkObject != null ? player.networkObject.HasStateAuthority.ToString() : "NULL")}");
        ThrowDebugLogger.LogThrow($"HasInputAuthority: {(player.networkObject != null ? player.networkObject.HasInputAuthority.ToString() : "NULL")}");
        ThrowDebugLogger.LogThrow($"Runner: {(networkObject.Runner != null ? "EXISTS" : "NULL")}");
        
        // Destroi o item antigo da mão, se houver
        if (myHandItem != null)
        {
            ThrowDebugLogger.LogThrow($"Removendo item antigo: {myHandItem.name}");
            
            if (myHandItem.TryGetComponent<NetworkObject>(out var netObj))
            {
                if (networkObject.Runner != null)
                {
                    networkObject.Runner.Despawn(netObj);
                }
                else
                {
                    Destroy(myHandItem);
                }
            }
            else
            {
                Destroy(myHandItem);
            }
            
            myHandItem = null;
        }

        if (selectedItem != null)
        {
            ThrowDebugLogger.LogThrow($"Equipando item: {selectedItem.itemName}");
            
            GameObject prefab = ItemDatabase.GetPrefabForItem(selectedItem);
            if (prefab != null)
            {
                bool shouldUseNetwork = networkObject.Runner != null && 
                                      player.networkObject != null && 
                                      player.networkObject.HasInputAuthority;
                
                ThrowDebugLogger.LogThrow($"Usando rede: {shouldUseNetwork}");
                
                if (shouldUseNetwork)
                {
                    try
                    {
                        NetworkObject newItemNet = networkObject.Runner.Spawn(
                            prefab, 
                            pickUpParent.position, 
                            pickUpParent.rotation, 
                            player.networkObject.InputAuthority
                        );
                        
                        newItemNet.transform.SetParent(pickUpParent);
                        
                        if (!Application.isEditor)
                        {
                            newItemNet.transform.localPosition = Vector3.zero;
                            newItemNet.transform.localRotation = Quaternion.identity;
                            ThrowDebugLogger.LogThrow("BUILD: Forçando posição local zero antes da configuração");
                            
                            StartCoroutine(EnsureParentingInBuild(newItemNet.gameObject, selectedItem));
                        }
                        else
                        {
                            ConfigureSpawnedItem(newItemNet.gameObject, selectedItem);
                        }
                        ThrowDebugLogger.LogThrow($"Item spawnado via rede com sucesso: {selectedItem.itemName}");
                    }
                    catch (System.Exception e)
                    {
                        ThrowDebugLogger.LogThrowError($"Erro ao spawnar via rede: {e.Message}");
                        SpawnItemLocally(prefab, selectedItem);
                    }
                }
                else
                {
                    ThrowDebugLogger.LogThrow("Spawnando item localmente (sem rede)");
                    SpawnItemLocally(prefab, selectedItem);
                }
            }
            else
            {
                ThrowDebugLogger.LogThrowError($"Prefab não encontrado para item: {selectedItem.name}");
                Debug.LogWarning("Prefab não encontrado para item: " + selectedItem.name);
            }
        }
        else
        {
            ThrowDebugLogger.LogThrow("Nenhum item selecionado - ocultando AmmoSlot");
            AmmoSlot.SetActive(false);
        }
    }

    void SpawnItemLocally(GameObject prefab, ItemSO selectedItem)
    {
        GameObject newItem = Instantiate(prefab, pickUpParent.position, pickUpParent.rotation, pickUpParent);
        ConfigureSpawnedItem(newItem, selectedItem);
        ThrowDebugLogger.LogThrow($"Item spawnado localmente: {selectedItem.itemName}");
    }

    void ConfigureSpawnedItem(GameObject newItem, ItemSO selectedItem)
    {
        if (newItem.transform.parent != pickUpParent)
        {
            ThrowDebugLogger.LogThrow($"CORREÇÃO: Item não estava parented - configurando parent para {pickUpParent.name}");
            newItem.transform.SetParent(pickUpParent);
        }
        
        if (!Application.isEditor)
        {
            newItem.transform.SetParent(null);
            newItem.transform.SetParent(pickUpParent);
            
            newItem.transform.localPosition = Vector3.zero;
            newItem.transform.localRotation = Quaternion.identity;
            newItem.transform.localScale = Vector3.one;
            
            ThrowDebugLogger.LogThrow($"BUILD: Parent forçado para {pickUpParent.name}, posição zerada");
        }
        
        Rigidbody rb = newItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }

        myHandItem = newItem;
        
        var debugHelper = FindFirstObjectByType<WeaponDebugHelper>();
        if (debugHelper != null && !Application.isEditor)
        {
            debugHelper.ResetBuildFixCounter();
        }

        if (newItem.TryGetComponent<Weapon>(out var weapon))
        {
            WeaponOffsetFixer fixer = newItem.GetComponent<WeaponOffsetFixer>();
            if (fixer == null)
            {
                fixer = newItem.AddComponent<WeaponOffsetFixer>();
            }
            
            fixer.SendMessage("ApplyDefaultOffsets", SendMessageOptions.DontRequireReceiver);
            
            newItem.transform.localPosition = weapon.Offset;
            newItem.transform.localRotation = weapon.OffsetRotation;
            
            if (player != null && player.aimActive)
            {
                newItem.transform.localPosition = weapon.AimOffset;
                newItem.transform.localRotation = weapon.AimOffsetRotation;
                ThrowDebugLogger.LogThrow($"Arma equipada já em modo mira - AimOffset: {weapon.AimOffset}");
            }
            
            ThrowDebugLogger.LogThrow($"Arma configurada com fixer - Tipo: {weapon.Type}, Offset: {weapon.Offset}");
        }

        // Configura dados da arma
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
                    
                    ThrowDebugLogger.LogThrow($"Arma equipada - Munição: {gun.CurrentAmmo}/{gun.MaxAmmo}");
                }
                else
                {
                    AmmoSlot.SetActive(true);
                    AmmoSlot.GetComponentInChildren<TextMeshProUGUI>()
                        .text = "<Arremessar>";
                    
                    ThrowDebugLogger.LogThrow("Item arremessável equipado");
                }
            }
            else
            {
                ThrowDebugLogger.LogThrowWarning($"Item não encontrado no database: {selectedItem.name}");
            }
        }

        Debug.Log("Equipado item na mão: " + selectedItem.itemName);
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
