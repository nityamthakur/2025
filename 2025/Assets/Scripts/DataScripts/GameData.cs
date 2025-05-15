using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int saveSlot;
    public int day;
    public int money, totalMoneyEarned, totalMoneySpent;
    public int rent;
    public float performanceScale;
    private bool hasUVLightUpgrade = false;
    private bool hasTimerUpgrade = false;
    public float PerformanceScale
    {
        get { return performanceScale; }
        set { performanceScale = Mathf.Clamp(value, 0f, 1f); }
    }
    public float playTime;
    public List<JobScene.Entry> releasedEmails = new();

    public GameData()
    {
        saveSlot = -1;
        day = 1;
        money = 0;
        totalMoneyEarned = 0;
        totalMoneySpent = 0;
        rent = 5;
        PerformanceScale = 0.5f;  // Default to 50% performance (0.5f)
        playTime = 0f;
    }

    public GameData(GameData loadedGame)
    {
        this.saveSlot = loadedGame.saveSlot;
        this.day = loadedGame.day;
        this.money = loadedGame.money;
        this.totalMoneyEarned = loadedGame.totalMoneyEarned;
        this.totalMoneySpent = loadedGame.totalMoneySpent;
        this.rent = loadedGame.rent;
        this.PerformanceScale = loadedGame.PerformanceScale;
        this.playTime = loadedGame.playTime;
        this.releasedEmails = loadedGame.releasedEmails;
    }

    public int GetCurrentDay()
    {
        return this.day;
    }
    public int GetCurrentMoney()
    {
        return this.money;
    }
    

    public void SetCurrentDay(int day)
    {
        this.day = day;
    }
    public void SetCurrentMoney(int money)
    {
        this.money = money;
    }

    public bool HasUVLightUpgrade()
    {
        return hasUVLightUpgrade;
    }
    public void SetUVLightUpgraded(bool upgraded)
    {
        hasUVLightUpgrade = upgraded;
    }
    public bool HasTimerUpgrade()
    {
        return hasTimerUpgrade;
    }
    public void SetTimerUpgraded(bool upgraded)
    {
        hasTimerUpgrade = upgraded;
    }

    public List<JobScene.Entry> LinkEmails()
    {
        return this.releasedEmails;

    }
}
