using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using Unity.VisualScripting;

public class ShopScene : MonoBehaviour
{
    [SerializeField] private GameObject shopScreenPrefab;

    [SerializeField] private int upgradePrice1 = 7; // UV Light upgrade price
    [SerializeField] private int upgradePrice2 = 5;
    [SerializeField] private string upgradeDescription1 = "UV Light Range+: Increases the detection radius of your UV light";
    [SerializeField] private string upgradeDescription2 = "Time Extension: Increases the allotted time for tasks";
    [SerializeField] private int[] upgradePrices1 = { 7, 10, 15 }; // UV Light prices per tier
    [SerializeField] private int[] upgradePrices2 = { 5, 8, 12 }; // Timer prices per tier
    [SerializeField] public GameObject cosmeticShopEntryPrefab; // Assign in Inspector
    [SerializeField] public CosmeticShopItem[] cosmeticItems;
    public CosmeticShopItem[] secondRowPurchasables;
    [System.Serializable]
    public class CosmeticShopItem
    {
        public string id; // Unique identifier
        public string displayName;
        public int price;
        public Sprite icon;
    }

    public GameObject currentShopScreen;
    private Button doneButton;
    private TextMeshProUGUI moneyText;

    // Upgrade UI elements
    private Button upgradeButton1, upgradeButton2, upgradeButton3;
    private TextMeshProUGUI upgradeText1, upgradeText2, upgradeText3;
    private Image upgradeImage1, upgradeImage2, upgradeImage3;
    private int playerMoney;
    private GameManager gameManager;
    private Image[] upgradeLevelBars1;
    private Image[] upgradeLevelBars2;
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
        // Find and set up UI components
        moneyText = currentShopScreen.transform.Find("MoneyText").GetComponent<TextMeshProUGUI>();
        moneyText.text = $"Money: {GetPlayerMoney()}";

        // Set up the Done button
        doneButton = currentShopScreen.transform.Find("DoneButton").GetComponent<Button>();
        doneButton.onClick.AddListener(FinishShopping);

        // Set up the shop title
        TextMeshProUGUI shopTitle = currentShopScreen.transform.Find("ShopTitle").GetComponent<TextMeshProUGUI>();
        shopTitle.text = "SHOP";

        // Only show UV Light upgrade after day 3
        int currentDay = gameManager.gameData.GetCurrentDay();
        Transform secondRow = currentShopScreen.transform.Find("SecondRow");
        Transform upgradeSlot1 = secondRow.transform.Find("UpgradeSlot1");
        Transform upgradeSlot3 = secondRow.transform.Find("UpgradeSlot3");
        if (currentDay >= 3)
        {
            if (upgradeSlot1 != null)
                upgradeSlot1.gameObject.SetActive(true);
            SetupUpgradeSlot(1, upgradePrice1, upgradeDescription1);
        }
        if (currentDay < 5)
        {
            if (upgradeSlot3 != null)
                upgradeSlot3.gameObject.SetActive(true);
            SetupUpgradeSlot(3, 50);
        }
        else
        {
            if (upgradeSlot1 != null)
                upgradeSlot1.gameObject.SetActive(false);
        }

        // Timer upgrade is always available
        SetupUpgradeSlot(2, upgradePrice2, upgradeDescription2);

        // Update button interactability based on available money and purchased status
        UpdateButtonStates();

        Transform cosmeticsPanel = currentShopScreen.transform.Find("CosmeticsPanel");
        if (cosmeticsPanel != null && cosmeticItems != null)
        {
            // Clear previous entries
            foreach (Transform child in cosmeticsPanel)
                Destroy(child.gameObject);

            int itemNumber = 111;
            foreach (var item in cosmeticItems)
            {
                GameObject entry = Instantiate(cosmeticShopEntryPrefab, cosmeticsPanel);
                // Set icon
                entry.transform.Find("Icon").GetComponent<Image>().sprite = item.icon;
                // Set name
                //entry.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.displayName;
                TextMeshProUGUI nameText = entry.transform.Find("Name").GetComponent<TextMeshProUGUI>();
                nameText.text = item.displayName;
                nameText.gameObject.SetActive(false);

                // Set price
                //entry.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = $"${item.price}";
                TextMeshProUGUI priceText = entry.transform.Find("Price").GetComponent<TextMeshProUGUI>();
                priceText.text = $"${item.price}";
                RectTransform priceRect = priceText.GetComponent<RectTransform>();
                priceRect.anchoredPosition -= new Vector2(0, 20);

                // Set button
                Button buyButton = entry.transform.Find("BuyButton").GetComponent<Button>();
                buyButton.interactable = !gameManager.gameData.IsCosmeticPurchased(item.id) && GetPlayerMoney() >= item.price;
                buyButton.interactable = false;
                buyButton.onClick.AddListener(() =>
                {
                    PurchaseCosmetic(item);
                    EventManager.PlaySound?.Invoke("buttonBeep", true);

                });
                // Optionally, disable button and show "OWNED" if purchased
                if (gameManager.gameData.IsCosmeticPurchased(item.id))
                {
                    buyButton.interactable = false;
                    buyButton.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "OWNED";
                }
                else
                {
                    TextMeshProUGUI buttonText = entry.transform.Find("BuyButton/Text (TMP)").GetComponent<TextMeshProUGUI>();
                    buttonText.color = Color.black;
                    buttonText.text = $"{itemNumber}";
                }
                itemNumber++;
            }
        }
    }

    private void PurchaseCosmetic(CosmeticShopItem item)
    {
        if (gameManager.gameData.IsCosmeticPurchased(item.id)) return;
        if (GetPlayerMoney() < item.price) return;

        SpendPlayerMoney(item.price);
        gameManager.gameData.PurchaseCosmetic(item.id);

        EventManager.VendingMachineItemFall?.Invoke($"{item.displayName}");

        // Update UI
        SetupShopUI();

        // Refresh cosmetics in the job scene if it exists
        JobScene jobScene = FindFirstObjectByType<JobScene>();
        if (jobScene != null)
        {
            jobScene.RefreshCosmetics();
        }
    }

    // For purchasing cosmetics via the keypad of VendingMachine
    private void PurchaseCosmeticById(string id)
    {
        var cosmetic = Array.Find(cosmeticItems, c => c.id == id);
        if (cosmetic != null)
        {
            PurchaseCosmetic(cosmetic);
        }
    }

    private void SetupUpgradeSlot(int slotNumber, int price, string description = null)
    {
        // Find the upgrade slot parent
        Transform secondRow = currentShopScreen.transform.Find("SecondRow");
        Transform upgradeSlot = secondRow.transform.Find($"UpgradeSlot{slotNumber}");
        if (upgradeSlot == null)
        {
            Debug.LogError($"UpgradeSlot{slotNumber} not found in shop prefab");
            return;
        }

        // Get button, text, and image components
        Button button = upgradeSlot.Find("BuyButton").GetComponent<Button>();

        TextMeshProUGUI priceText = upgradeSlot.Find("PriceText").GetComponent<TextMeshProUGUI>();
        RectTransform priceRect = priceText.GetComponent<RectTransform>();
        priceRect.anchoredPosition -= new Vector2(0, 20);

        TextMeshProUGUI descText = upgradeSlot.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        descText.gameObject.SetActive(false);

        Image image = upgradeSlot.Find("UpgradeImage").GetComponent<Image>();
        Transform upgradeLevel = upgradeSlot.Find("UpgradeLevel");

        Image[] levelBars = upgradeLevel != null ? upgradeLevel.GetComponentsInChildren<Image>() : null;
        if (levelBars != null)
        {
            foreach (Image bar in levelBars)
            {
                RectTransform rt = bar.GetComponent<RectTransform>();
                if (rt != null)
                    rt.anchoredPosition -= new Vector2(0, 20);
            }
        }


        // Store references for later use
        if (slotNumber == 1)
        {
            upgradeButton1 = button;
            upgradeText1 = priceText;
            upgradeImage1 = image;
            upgradeLevelBars1 = levelBars;
        }
        else if (slotNumber == 2)
        {
            upgradeButton2 = button;
            upgradeText2 = priceText;
            upgradeImage2 = image;
            upgradeLevelBars2 = levelBars;
        }
        else if (slotNumber == 3)
        {
            upgradeButton3 = button;
            upgradeImage3 = image;
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
        bool canAfford1 = GetPlayerMoney() >= upgradePrice1;
        bool canAfford2 = GetPlayerMoney() >= upgradePrice2;

        // Check if upgrades are already purchased
        bool isUVLightUpgradePurchased = gameManager != null &&
                                        gameManager.gameData != null &&
                                        gameManager.gameData.HasUVLightUpgrade();
        int numTimerUpgradesPurchased = gameManager.gameData.numPurchasedTimerUpgrades;

        int uvTier = gameManager.gameData.GetUVLightUpgradeTier();
        int timerTier = gameManager.gameData.GetTimerUpgradeTier();

        // Update price and button for UV Light
        if (upgradeButton1 != null)
        {
            bool maxed = uvTier >= 3;
            int nextPrice = !maxed ? upgradePrices1[uvTier] : 0;
            upgradeButton1.interactable = !maxed && GetPlayerMoney() >= nextPrice;
            upgradeText1.text = !maxed ? $"${nextPrice}" : "MAXED";
            upgradeText1.color = maxed ? Color.green : (GetPlayerMoney() >= nextPrice ? Color.white : Color.red);

            // Button text
            Transform buttonTextTrans = upgradeButton1.transform.Find("Text (TMP)");
            if (buttonTextTrans != null)
            {
                var buttonText = buttonTextTrans.GetComponent<TextMeshProUGUI>();
                buttonText.text = maxed ? "MAXED" : "BUY";
            }

            // Update bar
            UpdateUpgradeLevelBar(upgradeLevelBars1, uvTier);
        }

        // Timer upgrade
        if (upgradeButton2 != null)
        {
            bool maxed = timerTier >= 3;
            int nextPrice = !maxed ? upgradePrices2[timerTier] : 0;
            upgradeButton2.interactable = !maxed && GetPlayerMoney() >= nextPrice;
            upgradeText2.text = !maxed ? $"${nextPrice}" : "MAXED";
            upgradeText2.color = maxed ? Color.green : (GetPlayerMoney() >= nextPrice ? Color.white : Color.red);

            Transform buttonTextTrans = upgradeButton2.transform.Find("Text (TMP)");
            if (buttonTextTrans != null)
            {
                var buttonText = buttonTextTrans.GetComponent<TextMeshProUGUI>();
                buttonText.text = maxed ? "MAXED" : "BUY";
            }

            UpdateUpgradeLevelBar(upgradeLevelBars2, timerTier);
        }
    }
    private void UpdateUpgradeLevelBar(Image[] bars, int tier)
    {
        if (bars == null) return;
        for (int i = 0; i < bars.Length; i++)
        {
            bars[i].color = i < tier ? Color.yellow : Color.gray;
        }
    }

    private void PurchaseUpgrade(int upgradeNumber, int price)
    {
        // Play purchase sound
        EventManager.PlaySound?.Invoke("switch1", true);

        int tier = (upgradeNumber == 1) ? gameManager.gameData.GetUVLightUpgradeTier() : gameManager.gameData.GetTimerUpgradeTier();
        int[] prices = (upgradeNumber == 1) ? upgradePrices1 : upgradePrices2;
        if (tier >= 3 || GetPlayerMoney() < prices[tier]) return;

        SpendPlayerMoney(prices[tier]);

        // Update money display
        moneyText.text = $"Money: ${GetPlayerMoney()}";

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


    void OnEnable()
    {
        EventManager.PurchaseCosmeticById += PurchaseCosmeticById;
    }

    void OnDisable()
    {
        EventManager.PurchaseCosmeticById -= PurchaseCosmeticById;
    }
}