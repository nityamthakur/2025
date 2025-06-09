using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class VendingMachine : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayPanel;
    [SerializeField] private TextMeshProUGUI nameText, descriptionText, effectText, moneyText;
    [SerializeField] private Transform itemSlots;
    [SerializeField] private GameObject itemPrefab;
    private List<VendingMachineItem> vendingMachineItems;
    private GameManager gameManager;
    private ShopScene shopScene;
    private string itemCode;
    private int currentMoney;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        shopScene = FindFirstObjectByType<ShopScene>();
        itemCode = "";
        currentMoney = gameManager.gameData.GetCurrentMoney();
        moneyText.text = $"Money: {currentMoney}";

        LoadJsonFromFile();
        CreatePurchasables();
        UpdateDisplayPanel();
    }

    public void ConfirmPurchase()
    {
        EventManager.PlaySound?.Invoke("buttonBeep", true);
        ItemCheck();
        itemCode = "";
        UpdateDisplayPanel();
    }
    public void CancelButtonPanel()
    {
        EventManager.PlaySound?.Invoke("buttonBeep", true);
        itemCode = "";
        UpdateDisplayPanel();
    }
    public void InputItemCode(string code)
    {
        EventManager.PlaySound?.Invoke("buttonBeep", true);
        if (itemCode.Length > 2)
        {
            itemCode = "";
            UpdateDisplayPanel();
        }
        itemCode += code;
        UpdateDisplayPanel();
    }
    private void UpdateDisplayPanel(string grabbedCode = null)
    {
        if (grabbedCode == null)
            displayPanel.text = itemCode;
        else
            displayPanel.text = grabbedCode;

        if (itemCode.Length == 3)
            UpdateItemScreen();
    }

    private void UpdateItemScreen()
    {
        VendingMachineItem item = vendingMachineItems.Find(item => item.itemCode == itemCode);
            bool stockDayCheck = false;

            if (item != null && item.stockDay <= gameManager.gameData.GetCurrentDay())
                stockDayCheck = true;
            nameText.text = stockDayCheck ? item.itemName : "";
            descriptionText.text = stockDayCheck ? item.itemDescription : "";
            effectText.text = stockDayCheck ? item.itemEffect : "";
    }

    public void CreatePurchasables()
    {
        foreach (VendingMachineItem item in vendingMachineItems)
        {
            GameObject entry = Instantiate(itemPrefab, itemSlots);
            if (item.stockDay > gameManager.gameData.GetCurrentDay())
                entry.SetActive(false);

            // Locate item image and place it in the instantiated entry
            Sprite itemImage = Resources.Load<Sprite>($"Sprites/{item.itemImage}");
            entry.transform.Find("Image").GetComponent<Image>().sprite = itemImage;
            entry.transform.Find("FallingImage").GetComponent<Image>().sprite = itemImage;
            entry.transform.Find("FallingImage").GetComponent<Image>().gameObject.SetActive(false);

            // Grab how many times item has been purchased, and sets the appropriate cost
            TextMeshProUGUI costComponent = entry.transform.Find("Cost").GetComponent<TextMeshProUGUI>();

            int purchaseCount = gameManager.gameData.itemPurchases.TryGetValue(item.itemName, out int count) ? count : 0;
            if (item.itemCost.Length == count) // Already purchased, including items with multiple upgrades
                costComponent.gameObject.SetActive(false);
            else
                costComponent.text = $"${item.itemCost[purchaseCount]}";

            // Set upgrade levels 
                Slider upgradeBar = entry.transform.Find("UpgradeBar").GetComponent<Slider>();
            upgradeBar.maxValue = item.itemCost.Length;
            upgradeBar.value = purchaseCount;
            if (item.itemCost.Length <= 1) // Make inactive if not able to be purchased multiple times
                upgradeBar.gameObject.SetActive(false);

            // Set Button. Not going to be actually active
            Button buyButton = entry.transform.Find("BuyButton").GetComponent<Button>();
            buyButton.interactable = false;
            buyButton.transform.GetComponentInChildren<TextMeshProUGUI>().text = (item.itemCost.Length == count) ? "Sold Out" : item.itemCode;

            // Ensure that the reference gets set back to the item
            item.attachedUpgrade = entry;
        }
    }

    private void ItemCheck()
    {
        VendingMachineItem item = vendingMachineItems.Find(i => i.itemCode == itemCode);
        if (item == null)
        {
            //Debug.Log("Item not found with code: " + itemCode);
            return;
        }

        // Check if item can be purchased
        int purchaseCount = gameManager.gameData.itemPurchases.TryGetValue(item.itemName, out int count) ? count : 0;
        if (purchaseCount == item.itemCost.Length)
            return;

        // Check if player has enough money
        if (currentMoney < item.itemCost[purchaseCount])
            return;

        currentMoney -= item.itemCost[purchaseCount];
        moneyText.text = $"Money: {currentMoney}";

        if (gameManager.gameData.itemPurchases.ContainsKey(item.itemName))
            gameManager.gameData.itemPurchases[item.itemName]++;
        else
            gameManager.gameData.itemPurchases[item.itemName] = 1;

        gameManager.gameData.dailyItemPurchases.Add(new KeyValuePair<string, int>(item.itemName, item.itemCost[purchaseCount]));

        item.UpdateObjectInformation(gameManager.gameData);
        StartCoroutine(FallingItem(item));
    }

   private IEnumerator FallingItem(VendingMachineItem item)
    {
        StartCoroutine(DelaySound(1f));

        Image fallingImage = item.attachedUpgrade.transform.Find("FallingImage").GetComponent<Image>();
        fallingImage.gameObject.SetActive(true);

        float time = 0f;
        float duration = 1.0f;
        Vector3 start = fallingImage.transform.position;
        Vector3 end = start + Vector3.down * 7f;

        while (time < duration)
        {
            fallingImage.transform.position = Vector3.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        fallingImage.gameObject.SetActive(false);
        fallingImage.transform.position = start;
    }        

    private IEnumerator DelaySound(float time)
    {
        yield return new WaitForSeconds(time);
        EventManager.PlaySound?.Invoke("objectThunk", true);
    }

    private void LoadJsonFromFile()
    {
        // Check if Json is found in StreamingAssets folder
        string path = Path.Combine(Application.streamingAssetsPath, "GameText.json");
        if (!File.Exists(path))
        {
            Debug.LogError("JSON file not found at: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        ParseJson(json);
    }

    private void ParseJson(string json)
    {
        var wrapper = JsonUtility.FromJson<Wrapper>(json);
        if (wrapper != null && wrapper.vendingMachineItems != null && wrapper.vendingMachineItems.Count > 0)
        {
            vendingMachineItems = wrapper.vendingMachineItems;
        }
        else
        {
            Debug.LogError("Failed to parse JSON or empty purchasables.");
        }
    }


    [Serializable]
    private class Wrapper
    {
        public List<VendingMachineItem> vendingMachineItems;
    }

    [Serializable]
    public class VendingMachineItem
    {
        public string itemCode; // What number to press in vending machine for purchase and info
        public string itemName;
        public string itemDescription;
        public string itemEffect;
        public string itemImage;
        public int[] itemCost; // Array of costs, determines number of purchasable upgrades
        public int stockDay = 0; // What day it can appear in shop
        [NonSerialized] public GameObject attachedUpgrade;

        public void UpdateObjectInformation(GameData gameData)
        {
            int purchaseCount = gameData.itemPurchases.TryGetValue(itemName, out int count) ? count : 0;

            Slider upgradeBar = attachedUpgrade.transform.Find("UpgradeBar").GetComponent<Slider>();
            upgradeBar.maxValue = itemCost.Length;
            upgradeBar.value = purchaseCount;
            if (itemCost.Length <= 1) // Make inactive if not able to be purchased multiple times
                upgradeBar.gameObject.SetActive(false);

            TextMeshProUGUI costComponent = attachedUpgrade.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
            if (itemCost.Length == count) // Already purchased, including items with multiple upgrades
                costComponent.gameObject.SetActive(false);
            else
                costComponent.text = $"${itemCost[purchaseCount]}";


            Button buyButton = attachedUpgrade.transform.Find("BuyButton").GetComponent<Button>();
            if (itemCost.Length == count)
                buyButton.transform.GetComponentInChildren<TextMeshProUGUI>().text = "Sold Out";

        }

        public void Print()
        {
            Debug.Log($"{itemName}, {itemCode}, {itemCost}");
        }
    }
}