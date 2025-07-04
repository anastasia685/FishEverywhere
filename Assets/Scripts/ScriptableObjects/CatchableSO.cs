using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CatchableSO", menuName = "Scriptable Objects/CatchableSO")]
public class CatchableSO : ScriptableObject
{
    public string id;
    public string itemName;
    public List<EnvironmentType> environments;
    public List<int> environmentCounts;
    public Sprite itemIcon;
    public string itemDescription;
    public string itemLocation;

    [Header("Size Parameters")]
    public int baseSize;
    public int sizeRange;


    [ContextMenu("Generate Guid")]
    private void GenerateGuid()
    {
        id = Utils.GenerateGuid();
    }
}
