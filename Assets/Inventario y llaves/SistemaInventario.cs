using System.Collections.Generic;
using UnityEngine;

public class SistemaInventario : MonoBehaviour
{
    private Dictionary<DataItemInventario, InventoryItem> itemDiccionario;

    public static SistemaInventario Instance;
    public List<InventoryItem> inventario {  get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            inventario = new List<InventoryItem>();
            itemDiccionario = new Dictionary<DataItemInventario, InventoryItem>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public InventoryItem Get(DataItemInventario referencedata)
    {
        if(itemDiccionario.TryGetValue(referencedata, out InventoryItem item))
        {
            return item;
        }
        return null;
    }

    public void Add(DataItemInventario referenceData)
    {
        if (itemDiccionario.TryGetValue(referenceData, out InventoryItem item))
        {
            item.AddToStack(1);
        }
        else
        {
            InventoryItem newItem = new InventoryItem(referenceData);
            inventario.Add(newItem);
            itemDiccionario.Add(referenceData, newItem);
        }

    }

}