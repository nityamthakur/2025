using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

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
    public int saveSlot, day;
    public float playTime;
    public int money = 0, totalMoneyEarned, totalMoneySpent, rent;
    public int lastJobPay = 0;


    public float performanceScale;
    public float PerformanceScale
    {
        get { return performanceScale; }
        set { performanceScale = Mathf.Clamp(value, 0f, 1f); }
    }
    public List<JobScene.Entry> releasedEmails = new();
    public List<Review> articleReviews = new();
    public HashSet<int> reviewNotificationSeen = new();

    public Dictionary<string, int> itemPurchases = new();
    // For reading purchases made that day. Clears each day in dayEndScene
    public List<KeyValuePair<string, int>> dailyItemPurchases = new();
    public readonly float timerUpgradeAmount = 30f;

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

        this.itemPurchases = loadedGame.itemPurchases ?? new Dictionary<string, int>();
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
        //SetCurrentMoney(lastJobPay, false);
    }


    // Upgrades
    public bool IsItemPurchased(string itemName)
    {
        return itemPurchases.ContainsKey(itemName);
    }

    public int GetTimerUpgradeTier()
    {
        string timer = "Timerall"; // Json file name pushed in itemPurchases
        int num = itemPurchases.TryGetValue(timer, out int value) ? value : 0;
        Debug.Log($"GetTimerUpgradeTier: {num}");
        return num;
    }
    public bool HasTimerUpgrade()
    {
        string timer = "Timerall"; // Json file name pushed in itemPurchases
        bool hasTimer = itemPurchases.ContainsKey(timer);
        Debug.Log($"HasTimerUpgrade: {hasTimer}");
        return itemPurchases.ContainsKey(timer);
    }

    public int GetUVLightUpgradeTier()
    {
        string uvLight = "DoubleGood Battery"; // Json file name pushed in itemPurchases
        int num = itemPurchases.TryGetValue(uvLight, out int value) ? value : 0;
        Debug.Log($"GetUVLightUpgradeTier: {num}");
        return num;
    }
    public bool HasUVLightUpgrade()
    {
        string uvLight = "DoubleGood Battery"; // Json file name pushed in itemPurchases
        bool hasUV = itemPurchases.ContainsKey(uvLight);
        Debug.Log($"HasUVLightUpgrade: {hasUV}");
        return itemPurchases.ContainsKey(uvLight);
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
