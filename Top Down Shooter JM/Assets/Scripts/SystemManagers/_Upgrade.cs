using UnityEngine;

public class _Upgrade : ScriptableObject
{
    public string upgradeName;
    public UpgradeManager.upgradeTypes upgradeType;
    public Sprite upgradeImage;
    public float value;
    public string description;
    public int cost;
}
