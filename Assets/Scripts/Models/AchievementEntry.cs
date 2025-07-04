namespace Models
{
    public class AchievementEntry
    {
        public AchievementSO achievement;
        public int progress;

        public AchievementEntry(AchievementSO achievement, int progress)
        {
            this.achievement = achievement;
            this.progress = progress;
        }
    }
}
