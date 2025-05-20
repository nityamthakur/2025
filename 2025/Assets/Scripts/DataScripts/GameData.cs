using UnityEngine;
using System.Collections.Generic;
using System;

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
        this.rent = loadedGame.rent;
        this.PerformanceScale = loadedGame.PerformanceScale;
        this.playTime = loadedGame.playTime;
        this.releasedEmails = loadedGame.releasedEmails;
        this.releasedArticles = loadedGame.releasedArticles;
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

    public void SetRent(int rent)
    {
        this.rent += rent;
    }

    public void AddNewMedia(Media newMedia)
    {
        releasedArticles.Add(newMedia);
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
        if(releasedArticles.Count == 0)
            return 0;

        float articleTimeAvg = 0;
        foreach(Media media in releasedArticles)
        {
            articleTimeAvg += media.timeSpent;
        }
        articleTimeAvg /= releasedArticles.Count;
        return articleTimeAvg; 
    }

    public float MostTimeSpentOnArticle()
    {
        float time = 0;
        foreach(Media media in releasedArticles)
        {
            if(media.timeSpent > time)
                time =  media.timeSpent;
        }
        return time;   
    }

}
