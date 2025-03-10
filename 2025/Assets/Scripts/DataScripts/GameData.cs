using UnityEngine.Android;

[System.Serializable]
public class GameData
{
    public int day;
    public int money;
    public int rent;
    public int newMericaRep, greenPartyRep;
    public float playTime;

    public GameData()
    {
        day = 1;
        money = 0;
        rent = 3;
        newMericaRep = 50;
        greenPartyRep = 25;
        playTime = 0f;
    }

    public GameData(GameData loadedGame)
    {
        this.day = loadedGame.day;
        this.money = loadedGame.money;
        this.rent = loadedGame.rent;
        this.newMericaRep = loadedGame.newMericaRep;
        this.greenPartyRep = loadedGame.greenPartyRep;
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
}
