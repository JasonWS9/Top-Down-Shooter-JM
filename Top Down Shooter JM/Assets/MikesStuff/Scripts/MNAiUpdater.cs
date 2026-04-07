using System;
using UnityEngine;

public class MNAiUpdater : MonoBehaviour
{
    public static MNAiUpdater Instance;

    public float updateInterval = 20f;
    public float updateTimer;

    public static event Action OnUpdateAI;

    private float shotsFired;
    private float timeMoving;

    void Awake()
    {
        Instance = this;
        ResetStats();
    }

    void Update()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer < 0)
        {
            UpdateAi();
            ResetStats();
        }
    }

    void ResetStats()
    {
        updateTimer = updateInterval;
        shotsFired = 0;
        timeMoving = 0;
    }

    void UpdateAi()
    {
        OnUpdateAI?.Invoke();
    }

    public void Fire()
    {
        shotsFired++;
    }

    public void PlayerMoving()
    {
        timeMoving += Time.deltaTime;
    }
}