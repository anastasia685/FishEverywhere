using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models
{
    public class BestiaryEntry
    {
        public CatchableSO catchable;
        public float largestSize;
        public Dictionary<RarityType, int> caughtRarities;

        public BestiaryEntry(CatchableSO catchable, float largestSize, Dictionary<RarityType, int> caughtRarities)
        {
            this.catchable = catchable;
            this.largestSize = largestSize;
            this.caughtRarities = caughtRarities;
        }


        public static Dictionary<RarityType, int> InitialRarities = 
            Enum.GetValues(typeof(RarityType))
                .Cast<RarityType>()
                .ToDictionary(k => k, _k => 0);
    }
}
