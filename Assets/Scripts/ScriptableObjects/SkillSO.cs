using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "SkillSO", menuName = "Scriptable Objects/SkillSO")]
public class SkillSO : ScriptableObject
{
    public string id;
    public string skillName;
    public Sprite skillIcon;
    public string description;
    public int cost;
    /*public bool available = false;
    public bool active = false;*/

    public List<SkillSO> children;

    [Header("Buff config")]
    public BuffType buffType;
    public float buffMultiplier;


    [ContextMenu("Generate Guid")]
    private void GenerateGuid()
    {
        id = Utils.GenerateGuid();
    }
}