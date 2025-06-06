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
    public int lastJobPay = 0;
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
    public List<Review> articleReviews = new();
    public HashSet<int> reviewNotificationSeen = new();

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
        this.playTime = loadedGame.playTime;
        this.gameMode = loadedGame.gameMode;
        this.PerformanceScale = loadedGame.PerformanceScale;

        this.money = loadedGame.money;
        this.totalMoneyEarned = loadedGame.totalMoneyEarned;
        this.totalMoneySpent = loadedGame.totalMoneySpent;
        this.rent = loadedGame.rent;

        this.releasedEmails = loadedGame.releasedEmails;
        this.articleReviews = loadedGame.articleReviews;
        this.reviewNotificationSeen = loadedGame.reviewNotificationSeen ?? new HashSet<int>();

        this.numPurchasedTimerUpgrades = loadedGame.numPurchasedTimerUpgrades;
        this.hasUVLightUpgrade = loadedGame.hasUVLightUpgrade;
        this.purchasedCosmetics = loadedGame.purchasedCosmetics;
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
            GameMode.Easy => 2,
            GameMode.Normal => 4,
            GameMode.Hard => 6,
            _ => 2,
        };
        this.rent += difficultyScaler;
    }

    public void AddJobMoney(int money)
    {
        this.lastJobPay = money;
        SetCurrentMoney(lastJobPay, false);
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
        if (articleReviews.Count == 0)
            return 0;

        float articleWinRate = 0;
        foreach (Review media in articleReviews)
        {
            if (media.noMistakes)
                articleWinRate++;
        }
        articleWinRate /= articleReviews.Count;
        return MathF.Round(articleWinRate, 2);
    }

    public float ArticleTimeAverage()
    {
        if (articleReviews.Count == 0)
            return 0;

        float articleTimeAvg = 0;
        foreach (Review media in articleReviews)
        {
            articleTimeAvg += media.timeSpent;
        }
        articleTimeAvg /= articleReviews.Count;
        return articleTimeAvg;
    }

    public float MostTimeSpentOnArticle()
    {
        float time = 0;
        foreach (Review media in articleReviews)
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
        return timerUpgradeTier * timerUpgrade;
    }
    public bool IsCosmeticPurchased(string cosmeticId)
    {
        return purchasedCosmetics.Contains(cosmeticId);
    }

    public void PurchaseCosmetic(string cosmeticId)
    {
        purchasedCosmetics.Add(cosmeticId);
    }

    public void AddReviewToGameData(Entity.Newspaper newspaper)
    {
        Review newMedia = new()
        {
            title = newspaper.GetTitle(),
            publisher = newspaper.GetPublisher(),
            body = newspaper.GetFront() + "\n" + newspaper.GetBack(),
            date = newspaper.GetDate(),
            day = this.day,
            hiddenImageExists = newspaper.hasHiddenImage,
            censorWords = newspaper.censorWords,
            bannedWords = newspaper.banWords,
        };
        articleReviews.Add(newMedia);
    }
}
