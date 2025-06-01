using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class GameData
{
    public enum GameMode
    {
        Easy,
        Normal,
        Hard
    }
    public GameMode gameMode = GameMode.Normal;
    public int saveSlot;
    public int day;
    public int money, totalMoneyEarned, totalMoneySpent;
    public int rent;
    public float performanceScale;
    private bool hasUVLightUpgrade = false;
    private readonly float timerUpgrade = 30f;
    public int numPurchasedTimerUpgrades = 0;
    public HashSet<string> purchasedCosmetics = new HashSet<string>();
    private int uvLightUpgradeTier = 0;
    public int timerUpgradeTier = 0;
    public float PerformanceScale
    {
        get { return performanceScale; }
        set { performanceScale = Mathf.Clamp(value, 0f, 1f); }
    }
    public float playTime;
    public List<JobScene.Entry> releasedEmails = new();
    public List<Media> releasedArticles = new();

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
        this.numPurchasedTimerUpgrades = loadedGame.numPurchasedTimerUpgrades;
        this.rent = loadedGame.rent;
        this.PerformanceScale = loadedGame.PerformanceScale;
        this.playTime = loadedGame.playTime;
        this.releasedEmails = loadedGame.releasedEmails;
        this.releasedArticles = loadedGame.releasedArticles;
        this.gameMode = loadedGame.gameMode;
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
    public void SetCurrentMoney(int money, bool wasUpgrade)
    {
        this.money += money;
        if (money > 0)
            this.totalMoneyEarned += money;
        if (wasUpgrade)
            this.totalMoneySpent += Math.Abs(money);
    }

    public void IncreaseRent()
    {
        var difficultyScaler = gameMode switch
        {
            GameMode.Easy => 1,
            GameMode.Normal => 2,
            GameMode.Hard => 4,
            _ => 2,
        };
        this.rent += difficultyScaler;
    }

    public void AddNewMedia(Media newMedia)
    {
        releasedArticles.Add(newMedia);
    }

    public int GetUVLightUpgradeTier()
    {
        return uvLightUpgradeTier;
    }
    public void SetUVLightUpgradeTier(int tier)
    {
        uvLightUpgradeTier = Mathf.Clamp(tier, 0, 3);
    }
    public bool HasUVLightUpgrade()
    {
        return uvLightUpgradeTier > 0;
    }
    public void SetUVLightUpgraded(bool upgraded)
    {
        if (upgraded && uvLightUpgradeTier < 3)
            uvLightUpgradeTier++;
    }
    public List<JobScene.Entry> LinkEmails()
    {
        return this.releasedEmails;
    }


    public float ArticleWinRate()
    {
        if (releasedArticles.Count == 0)
            return 0;

        float articleWinRate = 0;
        foreach (Media media in releasedArticles)
        {
            if (media.noMistakes)
                articleWinRate++;
        }
        articleWinRate /= releasedArticles.Count;
        return MathF.Round(articleWinRate, 2);
    }

    public float ArticleTimeAverage()
    {
        if (releasedArticles.Count == 0)
            return 0;

        float articleTimeAvg = 0;
        foreach (Media media in releasedArticles)
        {
            articleTimeAvg += media.timeSpent;
        }
        articleTimeAvg /= releasedArticles.Count;
        return articleTimeAvg;
    }

    public float MostTimeSpentOnArticle()
    {
        float time = 0;
        foreach (Media media in releasedArticles)
        {
            if (media.timeSpent > time)
                time = media.timeSpent;
        }
        return time;
    }

    public int GetTimerUpgradeTier()
    {
        return timerUpgradeTier;
    }
    public void SetTimerUpgradeTier(int tier)
    {
        timerUpgradeTier = Mathf.Clamp(tier, 0, 3);
    }
    public bool HasTimerUpgrade()
    {
        return timerUpgradeTier > 0;
    }
    public void UpgradeTimer()
    {
        if (timerUpgradeTier < 3)
            timerUpgradeTier++;
    }
    internal float GetTimerUpgrade()
    {
        return timerUpgradeTier * 30f;
    }
    public bool IsCosmeticPurchased(string cosmeticId)
    {
        return purchasedCosmetics.Contains(cosmeticId);
    }

    public void PurchaseCosmetic(string cosmeticId)
    {
        purchasedCosmetics.Add(cosmeticId);
    }
}
