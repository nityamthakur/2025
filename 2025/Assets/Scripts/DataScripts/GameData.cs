using UnityEngine.Android;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int day;
    public int money;
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

    public GameData()
    {
        day = 1;
        money = 0;
        rent = 3;
        PerformanceScale = 0.5f;  // Default to 50% performance (0.5f)
        playTime = 0f;
    }

    public GameData(GameData loadedGame)
    {
        this.day = loadedGame.day;
        this.money = loadedGame.money;
        this.rent = loadedGame.rent;
        this.PerformanceScale = loadedGame.PerformanceScale;
        this.playTime = loadedGame.playTime;
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
}
