using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesSO : MonoBehaviour
{
    // Start is called before the first frame update
    public string upgradeName;
    public string description;
    public int upgradeId = 0;
    public float rarityMult = 0.0F;
    public float sizeMult = 0.0F;
    public int cost = 0;

    // upgrade id used for dependancy checks


    [Header("parameters")]
    public float rarity_addition;
    // adds a multiplyer to determining rarity
    //public int[] dependancies;
    // list of upgrades needed before this upgrade can be unlocked
    public bool isActive = true;
    // is the upgrade active or not




    static void Main(string[] args)
    {
        UpgradesSO upgrade0 = new UpgradesSO();
        upgrade0.upgradeName = "Base Fishing Rod";
        upgrade0.description = "A neat fishing rod that someone left behind";
        upgrade0.isActive = true;
        upgrade0.rarityMult = 1F;
        upgrade0.sizeMult = 1F;



        // creation of new upgrade and assigning values
        UpgradesSO upgrade1 = new UpgradesSO();
        upgrade1.upgradeName = "Bronze Fishing Rod";
        upgrade1.description = "A more hardy fishing rod able to catch slightly bigger and better fish";
        upgrade1.upgradeId = 0;
        upgrade1.isActive = false;
        upgrade1.rarityMult = 0.5F;
        upgrade1.sizeMult = 0.5F;
        upgrade1.cost = 1;


        //upgrade1.dependancies = new int[] { };
        // no dependancies as this is the first upgrade
        

        UpgradesSO upgrade2 = new UpgradesSO();
        upgrade2.upgradeName = "Iron Fishing Rod";
        upgrade2.description = "An even better fishing rod that can catche even bigger and better fish";
        upgrade2.upgradeId = 1;
        upgrade2.isActive = false;
        upgrade2.rarityMult = 0.5F;
        upgrade2.sizeMult = 1F;
        upgrade2.cost = 2;
        //upgrade2.dependancies = new int[1] { upgrade1.upgradeId };


        UpgradesSO upgrade3 = new UpgradesSO();
        upgrade3.upgradeName = "Gold Fishing Rod";
        upgrade3.description = "A shiny new fishing rod that can catch almost any size fish!";
        upgrade3.upgradeId = 2;
        upgrade3.isActive = false;
        upgrade3.rarityMult = 0.55F;
        upgrade3.sizeMult = 1.5F;
        upgrade3.cost = 3;
        //upgrade3.dependancies = new int[2] { upgrade1.upgradeId, upgrade2.upgradeId };


        UpgradesSO upgrade4 = new UpgradesSO();
        upgrade4.upgradeName = "Diamond Fishing Rod";
        upgrade4.description = "Serious dedication to the craft, this fishing rod is the best of the best";
        upgrade4.upgradeId = 3;
        upgrade4.isActive = false;
        upgrade4.rarityMult = 0.6F;
        upgrade4.sizeMult = 2F;
        upgrade4.cost = 4;
        //upgrade4.dependancies = new int[3] {upgrade1.upgradeId, upgrade2.upgradeId, upgrade3.upgradeId};
    }
}
