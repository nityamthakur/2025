using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
	public static AnalyticsManager Instance;
	private bool isInitialized = false;
    private void Awake()
    {
		if(Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
		}
    }

    private async void Start()
    {
		await UnityServices.InitializeAsync();	
		AnalyticsService.Instance.StartDataCollection();
		isInitialized = true;
    }

	public void GameStart()
	{
		if(!isInitialized)
		{
			return;
		}
		
		AnalyticsService.Instance.RecordEvent("gameStart");
		AnalyticsService.Instance.Flush();
		Debug.Log("gameStarted");
	}

	public void GameQuit()
	{
		if(!isInitialized)
		{
			return;
		}
		
		AnalyticsService.Instance.RecordEvent("gameQuit");
		AnalyticsService.Instance.Flush();
	}

	public void GameOver(GameData gameData)
	{
		if(!isInitialized)
		{
			return;
		}
		
		CustomEvent dayReached = new("dayReached")
		{
			{"dayReached", gameData.day}
		};
		AnalyticsService.Instance.RecordEvent(dayReached);

		CustomEvent timePlayed = new("timePlayed")
		{
			{"timePlayed", gameData.playTime}
		};
		AnalyticsService.Instance.RecordEvent(timePlayed);
		AnalyticsService.Instance.Flush();

		Debug.Log("GameOver");
	}

}