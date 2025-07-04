using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "HiddenAreaSO", menuName = "Scriptable Objects/HiddenAreaSO")]
public class HiddenAreaSO : ScriptableObject
{
    public string id;
    public Vector3Int location;

    [ContextMenu("Generate Guid")]
    private void GenerateGuid()
    {
        id = Utils.GenerateGuid();
    }
}