namespace Models
{
    public class HiddenAreaEntry
    {
        public HiddenAreaSO hiddenArea;
        public bool collected;

        public HiddenAreaEntry(HiddenAreaSO hiddenArea, bool collected)
        {
            this.hiddenArea = hiddenArea;
            this.collected = collected;
        }
    }
}
