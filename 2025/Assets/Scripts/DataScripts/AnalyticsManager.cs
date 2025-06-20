using System.Collections.Generic;
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
		//AnalyticsService.Instance.StartDataCollection();
		isInitialized = true;
    }

	public void GameOver(GameData gameData, int gameRating)
	{
		if(!isInitialized)
		{
			return;
		}
		
		CustomEvent gameEnd = new("GameOver")
		{
			//{"gameDifficulty", gameData.gameMode}
			{ "gameRating", gameRating},
			{"dayReached", gameData.day},
			{"timePlayed", gameData.playTime},
			{"moneyEarned", gameData.totalMoneyEarned},
			{"moneySpent", gameData.totalMoneySpent},
			{"moneyEndedWith", gameData.money},
			{"articlesSeen", gameData.articleReviews.Count},
			{"articleWinRate", gameData.ArticleWinRate()},
			{"articleTimeAvg", gameData.ArticleTimeAverage()},
			{"mostTimeSpentOnArticle", gameData.MostTimeSpentOnArticle()}
		};
		AnalyticsService.Instance.RecordEvent(gameEnd);
		AnalyticsService.Instance.Flush();

		Debug.Log("GameOver reached in AnalyitcsManager");
	}

	public void ArticleAnalysis(List<Review> articles)
	{
		if(!isInitialized)
		{
			return;
		}

		foreach(Review review in articles)
		{
			CustomEvent customEvent = new("articleAnalysis")
			{
				{"onDay", review.day},
				{"timeSpent", review.timeSpent},
				{"noMistakes", review.noMistakes},
			};
			AnalyticsService.Instance.RecordEvent(customEvent);

		}
		AnalyticsService.Instance.Flush();

		Debug.Log("ArticleAnalysis reached in AnalyitcsManager");
	}
}