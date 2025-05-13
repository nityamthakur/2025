using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject shopScreenPrefab;

    [SerializeField] private int upgradePrice1 = 7; // UV Light upgrade price
    [SerializeField] private int upgradePrice2 = 5;
    [SerializeField] private string upgradeDescription1 = "UV Light Range+: Increases the detection radius of your UV light";
    [SerializeField] private string upgradeDescription2 = "Time Extension: Increases the allotted time for tasks";

    private GameObject currentShopScreen;
    private Button doneButton;
    private TextMeshProUGUI moneyText;

    // Upgrade UI elements
    private Button upgradeButton1; // UV Light upgrade button
    private Button upgradeButton2;
    private TextMeshProUGUI upgradeText1;
    private TextMeshProUGUI upgradeText2;
    private Image upgradeImage1;
    private Image upgradeImage2;

    private int playerMoney;
    private GameManager gameManager;

    private void OnEnable()
    {
        // Subscribe to the GoToShop event
        EventManager.GoToShop += ShowShop;
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled to prevent memory leaks
        EventManager.GoToShop -= ShowShop;
    }

    private void ShowShop(int money)
    {
        // Store the player's money
        playerMoney = money;

        // Get reference to GameManager
        gameManager = FindFirstObjectByType<GameManager>();

        // Instantiate the shop screen prefab
        currentShopScreen = Instantiate(shopScreenPrefab);

        // Set up the shop UI
        SetupShopUI();

        // Fade in the shop screen
        EventManager.FadeIn?.Invoke();
    }

    private void SetupShopUI()
    {
        // Find and set up UI components
        moneyText = currentShopScreen.transform.Find("MoneyText").GetComponent<TextMeshProUGUI>();
        moneyText.text = $"Money: ${playerMoney}";

        // Set up the Done button
        doneButton = currentShopScreen.transform.Find("DoneButton").GetComponent<Button>();
        doneButton.onClick.AddListener(FinishShopping);

        // Set up the shop title
        TextMeshProUGUI shopTitle = currentShopScreen.transform.Find("ShopTitle").GetComponent<TextMeshProUGUI>();
        shopTitle.text = "SHOP";

        // Set up the upgrade buttons and images
        SetupUpgradeSlot(1, upgradePrice1, upgradeDescription1);
        SetupUpgradeSlot(2, upgradePrice2, upgradeDescription2);

        // Update button interactability based on available money and purchased status
        UpdateButtonStates();
    }

    private void SetupUpgradeSlot(int slotNumber, int price, string description)
    {
        // Find the upgrade slot parent
        Transform upgradeSlot = currentShopScreen.transform.Find($"UpgradeSlot{slotNumber}");
        if (upgradeSlot == null)
        {
            Debug.LogError($"UpgradeSlot{slotNumber} not found in shop prefab");
            return;
        }

        // Get button, text, and image components
        Button button = upgradeSlot.Find("BuyButton").GetComponent<Button>();
        TextMeshProUGUI priceText = upgradeSlot.Find("PriceText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = upgradeSlot.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        Image image = upgradeSlot.Find("UpgradeImage").GetComponent<Image>();

        // Store references for later use
        if (slotNumber == 1)
        {
            upgradeButton1 = button;
            upgradeText1 = priceText;
            upgradeImage1 = image;
        }
        else if (slotNumber == 2)
        {
            upgradeButton2 = button;
            upgradeText2 = priceText;
            upgradeImage2 = image;
        }

        // Set the texts
        priceText.text = $"${price}";
        descText.text = description;

        // Set up the button click handler
        int slotNum = slotNumber; // Capture for lambda
        button.onClick.AddListener(() => PurchaseUpgrade(slotNum, price));
    }

    private void UpdateButtonStates()
    {
        // Check if player can afford each upgrade and if it's already purchased
        bool canAfford1 = playerMoney >= upgradePrice1;
        bool canAfford2 = playerMoney >= upgradePrice2;

        // Check if upgrades are already purchased
        bool isUVLightUpgradePurchased = gameManager != null &&
                                        gameManager.gameData != null &&
                                        gameManager.gameData.HasUVLightUpgrade();
        bool isTimerUpgradePurchased = gameManager != null &&
                                    gameManager.gameData != null &&
                                    gameManager.gameData.HasTimerUpgrade();

        // Update button states
        if (upgradeButton1 != null)
        {
            upgradeButton1.interactable = canAfford1 && !isUVLightUpgradePurchased;
            upgradeText1.color = isUVLightUpgradePurchased ? Color.green : (canAfford1 ? Color.white : Color.red);

            // Update button text if already purchased
            if (isUVLightUpgradePurchased)
            {
                Transform buttonTextTrans = upgradeButton1.transform.Find("Text (TMP)");
                if (buttonTextTrans != null)
                {
                    TextMeshProUGUI buttonText = buttonTextTrans.GetComponent<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "PURCHASED";
                    }
                }
            }
        }

        if (upgradeButton2 != null)
        {
            upgradeButton2.interactable = canAfford2 && !isTimerUpgradePurchased;
            upgradeText2.color = isTimerUpgradePurchased ? Color.green : (canAfford2 ? Color.white : Color.red);

            if (isTimerUpgradePurchased)
            {
                Transform buttonTextTrans = upgradeButton2.transform.Find("Text (TMP)");
                if (buttonTextTrans != null)
                {
                    TextMeshProUGUI buttonText = buttonTextTrans.GetComponent<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "PURCHASED";
                    }
                }
            }
        }
    }

    private void PurchaseUpgrade(int upgradeNumber, int price)
    {
        // Play purchase sound
        EventManager.PlaySound?.Invoke("switch1");

        // Deduct money
        playerMoney -= price;

        // Update money display
        moneyText.text = $"Money: ${playerMoney}";

        // Apply the upgrade effect (store in GameManager.gameData)
        ApplyUpgradeEffect(upgradeNumber);

        // Update button states after purchase
        UpdateButtonStates();
    }

    private void ApplyUpgradeEffect(int upgradeNumber)
    {
        if (gameManager != null && gameManager.gameData != null)
        {
            switch (upgradeNumber)
            {
                case 1:
                    // Apply UV Light upgrade
                    gameManager.gameData.SetUVLightUpgraded(true);

                    // Find and enhance UV Light if it exists in the scene
                    EnhanceUVLight();

                    Debug.Log("UV Light upgrade purchased!");
                    break;
                case 2:
                    gameManager.gameData.SetTimerUpgraded(true);
                    Debug.Log("Timer Extension upgrade purchased!");
                    break;
            }

            // Be sure to set the current money in gameData before exiting shop
            gameManager.gameData.SetCurrentMoney(playerMoney);
        }
        else
        {
            Debug.LogError("GameManager or gameData is null when trying to apply upgrade");
        }
    }

    private void EnhanceUVLight()
    {
        // First make sure the upgrade is saved in game data
        if (gameManager != null && gameManager.gameData != null)
        {
            gameManager.gameData.SetUVLightUpgraded(true);
        }

        // Find all UVLight instances in the scene using the non-deprecated method
        UVLight[] uvLights = FindObjectsByType<UVLight>(FindObjectsSortMode.None);

        if (uvLights != null && uvLights.Length > 0)
        {
            // Apply the upgrade to each UV light found
            foreach (UVLight uvLight in uvLights)
            {
                if (uvLight != null)
                {
                    // Increase the radius of the UV light
                    uvLight.IncreaseRadius();
                    Debug.Log("UV Light upgrade applied to: " + uvLight.gameObject.name);
                }
            }
        }
        else
        {
            // If no UV Light exists in the current scene, just save the upgrade state
            // It will be applied when a UV Light is created
            Debug.Log("No UVLight found in scene - upgrade saved and will apply when UV Light is used");
        }
    }

    private void FinishShopping()
    {
        // Make sure the money amount is saved to game data
        if (gameManager != null && gameManager.gameData != null)
        {
            gameManager.gameData.SetCurrentMoney(playerMoney);
        }

        // Play sound effect
        EventManager.PlaySound?.Invoke("switch1");

        // Fade out
        EventManager.FadeOut?.Invoke();

        // Wait for fade out, then clean up and transition
        StartCoroutine(CleanupAndTransition());
    }

    private IEnumerator CleanupAndTransition()
    {
        // Wait for fade out
        yield return new WaitForSeconds(2f);

        // Clean up the shop UI
        Destroy(currentShopScreen);
        currentShopScreen = null;

        // Call the static method to transition to the next day
        DayEndScene.TransitionFromShop();
    }
}