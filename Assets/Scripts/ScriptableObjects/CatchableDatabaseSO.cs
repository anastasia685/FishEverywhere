using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CatchableDatabaseSO", menuName = "Scriptable Objects/CatchableDatabaseSO")]
public class CatchableDatabaseSO : ScriptableObject
{
    public List<CatchableSO> Entries;
}
