using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using Unity.VisualScripting;

public class ShopScene : MonoBehaviour
{
    [SerializeField] private GameObject shopScreenPrefab;
    public GameObject currentShopScreen;
    private Button doneButton;

    // Upgrade UI elements
    private GameManager gameManager;
    private bool isCleaningUp = false;

    public void Initalize()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void LoadShop()
    {
        // Get reference to GameManager
        gameManager = FindFirstObjectByType<GameManager>();

        // Instantiate the shop screen prefab
        currentShopScreen = Instantiate(shopScreenPrefab);
        Canvas prefabCanvas = currentShopScreen.GetComponentInChildren<Canvas>();
        if (prefabCanvas != null)
        {
            prefabCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            prefabCanvas.worldCamera = Camera.main;
        }

        // Set up the shop UI
        SetupShopUI();

        // Fade in the shop screen
        EventManager.FadeIn?.Invoke();
    }

    private void SetupShopUI()
    {
        // Set up the Done button
        doneButton = currentShopScreen.transform.Find("DoneButton").GetComponent<Button>();
        doneButton.onClick.AddListener(FinishShopping);

        // Set up the shop title
        TextMeshProUGUI shopTitle = currentShopScreen.transform.Find("ShopTitle").GetComponent<TextMeshProUGUI>();
        shopTitle.text = "SHOP";
    }

    public void CompletePurchase(string itemName)
    {

    }

    private void ApplyUpgradeEffect(int upgradeNumber)
    {
        if (gameManager != null && gameManager.gameData != null)
        {
            switch (upgradeNumber)
            {
                case 1:
                    int uvTier = gameManager.gameData.GetUVLightUpgradeTier();
                    if (uvTier < 3)
                    {
                        gameManager.gameData.SetUVLightUpgradeTier(uvTier + 1);
                        EnhanceUVLight();
                    }
                    break;
                case 2:
                    int timerTier = gameManager.gameData.GetTimerUpgradeTier();
                    if (timerTier < 3)
                    {
                        gameManager.gameData.SetTimerUpgradeTier(timerTier + 1);
                    }
                    break;
            }
        }
        else
        {
            Debug.LogError("GameManager or gameData is null when trying to apply upgrade");
        }
    }

    private void EnhanceUVLight()
    {
        // Do NOT increment the tier here!
        // Just apply the effect to all UVLight instances

        UVLight[] uvLights = FindObjectsByType<UVLight>(FindObjectsSortMode.None);

        if (uvLights != null && uvLights.Length > 0)
        {
            foreach (UVLight uvLight in uvLights)
            {
                if (uvLight != null)
                {
                    uvLight.IncreaseRadius();
                    Debug.Log("UV Light upgrade applied to: " + uvLight.gameObject.name);
                }
            }
        }
        else
        {
            Debug.Log("No UVLight found in scene - upgrade saved and will apply when UV Light is used");
        }
    }

    private int GetPlayerMoney()
    {
        return gameManager.gameData.GetCurrentMoney();
    }

    private void SpendPlayerMoney(int money)
    {
        gameManager.gameData.SetCurrentMoney(-money, true);
    }

    public IEnumerator NextScene()
    {
        EventManager.DisplayMenuButton?.Invoke(false);
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);

        Destroy(currentShopScreen);
        currentShopScreen = null;

        yield return new WaitForSeconds(3f);
        EventManager.NextScene?.Invoke();

        isCleaningUp = false;
    }

    private void FinishShopping()
    {
        if (isCleaningUp) return; // Prevent double cleanup/transition
        isCleaningUp = true;

        EventManager.PlaySound?.Invoke("switch1", true);
        EventManager.FadeOut?.Invoke();
        StartCoroutine(NextScene());
    }

    private T FindObject<T>(string name) where T : Component
    {
        return FindComponentByName<T>(name);
    }

    private T FindComponentByName<T>(string name) where T : Component
    {
        T[] components = GetComponentsInChildren<T>(true); // Search all children, even inactive ones

        foreach (T component in components)
        {
            if (component.gameObject.name == name)
                return component;
        }

        Debug.LogWarning($"Component '{name}' not found!");
        return null;
    }
}