using System;
using System.IO;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    public static PathManager Instance;

    public enum Alligences
    {
        neutral,
        alliance,
        aliens
    }

    public Alligences currentAlligence;

    private float currentReputation;

    public float neutralReputationRange;
    public float maxReputation;
    public float minReputation;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentAlligence = Alligences.neutral;
    }

    public void IncreaseValue(float value)
    {
        currentReputation += value;
        currentReputation = MathF.Min(currentReputation, maxReputation);
        UpdatePath();
    }

    public void DecreaseValue(float value)
    {
        currentReputation -= value;
        currentReputation = MathF.Max(currentReputation, minReputation);
        UpdatePath();
    }

    void UpdatePath()
    {
        if (currentReputation > neutralReputationRange)
        {
            currentAlligence = Alligences.alliance;
        } else if (currentReputation < -neutralReputationRange)
        {
            currentAlligence = Alligences.aliens;
        } else
        {
            currentAlligence = Alligences.neutral;
        }
    }
}
