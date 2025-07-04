using UnityEngine;

[CreateAssetMenu(fileName = "MapSectionSO", menuName = "Scriptable Objects/MapSectionSO")]
public class MapSectionSO : ScriptableObject
{
    public string sceneName; // in case we want to have multiple sections with same environment type
    //public EnvironmentType environmentType;
    public MapSectionSO up;
    public MapSectionSO down;
    public MapSectionSO left;
    public MapSectionSO right;
}
