using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "AchievementSO", menuName = "Scriptable Objects/AchievementSO")]
public class AchievementSO : ScriptableObject
{
    public string id;
    public AchievementType type;
    public string achievementName;
    public Sprite achievementIcon;
    public string description;

    [Header("Achievement Levels")]
    /*public int bronzeBar;
    public int silverBar;
    public int goldBar;*/
    public List<int> levels;

    [ContextMenu("Generate Guid")]
    private void GenerateGuid()
    {
        id = Utils.GenerateGuid();
    }
}