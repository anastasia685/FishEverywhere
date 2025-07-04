using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AchievementsDatabaseSO", menuName = "Scriptable Objects/AchievementsDatabaseSO")]
public class AchievementsDatabaseSO : ScriptableObject
{
    public List<AchievementSO> Entries;
}
