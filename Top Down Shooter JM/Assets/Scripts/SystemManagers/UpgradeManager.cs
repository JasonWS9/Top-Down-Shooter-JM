using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{

    public int currentXP;
    public int xpForUpgrade;
    public int xpRequirementIncrease;

    public static UpgradeManager Instance;

    private List<_Upgrade> allUpgrades;

    public enum upgradeTypes
    {
        speed,
        damage,
        health,
        other
    }


    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (currentXP >= xpForUpgrade)
        {
            currentXP = 0;
            IncreaseXPRequirement();
            GenerateUpgradeStep1();
        }
    }

    void IncreaseXPRequirement()
    {
        xpForUpgrade += xpRequirementIncrease;
    }

    void GenerateUpgradeStep1()
    {
        switch(PathManager.Instance.currentAlligence)
        {
            case PathManager.Alligences.neutral:
                GenerateUpgradeStep2(upgradeTypes.speed, upgradeTypes.damage);
                break;
            case PathManager.Alligences.alliance:
                GenerateUpgradeStep2(upgradeTypes.speed, upgradeTypes.health);
                break;
            case PathManager.Alligences.aliens:
                GenerateUpgradeStep2(upgradeTypes.health, upgradeTypes.damage);
                break;
        }

    }

    void GenerateUpgradeStep2(upgradeTypes upgradeType1, upgradeTypes upgradeType2)
    {
        List<_Upgrade> finalUpgradeList = new List<_Upgrade>();

        foreach (_Upgrade upgrade in allUpgrades)
        {
            if (upgrade.upgradeType == upgradeType1 || upgrade.upgradeType == upgradeType2)
            {
                finalUpgradeList.Add(upgrade);
            }
        }

        GenerateUpgradeStep3(finalUpgradeList);
    }

    _Upgrade GenerateUpgradeStep3(List<_Upgrade> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.Log("Cant find upgrade (step3)");
            return null;
        }

        int maxValue = list.Count;
        _Upgrade finalUpgrade = new _Upgrade();

        int random = UnityEngine.Random.Range(0, maxValue);

        finalUpgrade = list[random];

        return finalUpgrade;
    }
}
