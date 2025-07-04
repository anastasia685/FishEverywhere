using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public class SkillEntry
    {
        public SkillSO skill;
        public bool available = false;
        public bool active = false;

        public SkillEntry(SkillSO skill, bool available, bool active)
        {
            this.skill = skill;
            this.available = available;
            this.active = active;
        }
    }
}
