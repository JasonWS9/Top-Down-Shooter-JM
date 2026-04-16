using System;
using UnityEngine;
using UnityEngine.Events;

public class AiUpdater : MonoBehaviour
{

    public static AiUpdater Instance;

    public float updateInterval = 20f;
    public float updateTimer;

    public static event Action OnUpdateAI;

    private float shotsFired;
    private float timeMoving;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
        ResetStats();
    }

    // Update is called once per frame
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
        Debug.Log("Resetting");
        Debug.Log("Shots Fired: " + shotsFired);
        Debug.Log("Time Spent Moving " + timeMoving);

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
