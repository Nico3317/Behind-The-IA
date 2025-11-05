using UnityEngine;

[CreateAssetMenu(menuName = "Data Item Inventario")]
public class DataItemInventario : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
}
