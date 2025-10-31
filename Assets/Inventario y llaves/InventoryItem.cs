using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public DataItemInventario Data { get; private set; }

    public int StackSize { get; private set; }

    public InventoryItem(DataItemInventario data)
    {
        Data = data;
        AddToStack(1);
    }

    public void AddToStack(int amount)
    {
        StackSize += amount;
    }

}