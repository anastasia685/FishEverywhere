using UnityEngine;

[CreateAssetMenu(fileName = "BeastiaryEntrySO", menuName = "Scriptable Objects/BeastiaryEntrySO")]
public class BeastiaryEntrySO : ScriptableObject
{
    public CatchableSO Fish;
    public string Description;
    public string Location;

    [Header("Caught Fish")]
    public float LargestCaught;
    public int CommonCaught;
    public int RareCaught;
    public int EpicCaught;
}
