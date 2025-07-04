namespace Serializable
{
    [System.Serializable]
    public class BestiaryEntry
    {
        public string catchableId;
        public float largestSize;
        public Serializable.Dictionary<RarityType, int> caughtRarities;
    }
}
