using System.Collections;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int day;
    public int money;
    public float playTime;

    public GameData()
    {
        day = 1;
        money = 0;
        playTime = 0f;
    }

    public GameData(GameData loadedGame)
    {
        this.day = loadedGame.day;
        this.money = loadedGame.money;
        this.playTime = loadedGame.playTime;
    }

    private IEnumerator UpdatePlayTime()
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second
        playTime += 1f; // Increment playtime
    }
}
