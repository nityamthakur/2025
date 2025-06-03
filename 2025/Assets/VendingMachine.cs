using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class VendingMachine : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayPanel;
    [SerializeField] private TextMeshProUGUI nameText, descriptionText, effectText;
    [SerializeField] private Transform cosmeticsPanel, spawnLayer;
    private GameManager gameManager;
    private ShopScene shopScene;

    private List<VendingMachineItem> vendingMachineItems;
    private string itemCode = "";

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        shopScene = FindFirstObjectByType<ShopScene>();

        LoadJsonFromFile();
        CreatePurchasables();
        itemCode = "";
        UpdateDisplayPanel();
    }

    public void ConfirmPurchase()
    {
        EventManager.PlaySound?.Invoke("buttonBeep", true);
        ItemFall();
        CancelButtonPanel();
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
            CancelButtonPanel();
        itemCode += code;
        UpdateDisplayPanel();
    }
    private void UpdateDisplayPanel()
    {
        displayPanel.text = itemCode;
        VendingMachineItem item = vendingMachineItems.Find(item => item.itemCode == itemCode);
        bool stockDayCheck = false;
        if (item != null && item.stockDay <= gameManager.gameData.GetCurrentDay())
            stockDayCheck = true;
        nameText.text = stockDayCheck ? item.itemName : "";
        descriptionText.text = stockDayCheck ? item.itemDescription : "";
        effectText.text = stockDayCheck ? item.itemEffect : "";
    }

    private void ItemFall()
    {
        VendingMachineItem item = vendingMachineItems.Find(i => i.itemCode == itemCode);
        if (item == null || item.spawnPosition == null || item.itemImage == null)
            return;

        // Find the matching cosmetic item by name
        var cosmetic = Array.Find(shopScene.cosmeticItems, c => c.displayName == item.itemName);
        if (cosmetic == null)
        {
            return;
        }

        EventManager.PurchaseCosmeticById?.Invoke(cosmetic.id);

        GameObject fallingObject = new GameObject("FallingItem_" + item.itemName);

        fallingObject.transform.position = item.spawnPosition.position;
        fallingObject.transform.SetParent(spawnLayer);
        fallingObject.transform.localScale = Vector3.one / 2;

        Image image = fallingObject.AddComponent<Image>();
        image.sprite = item.itemImage;
        image.preserveAspect = true;

        Rigidbody2D rb = fallingObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;

        fallingObject.AddComponent<BoxCollider2D>();

        Destroy(fallingObject, 2f);
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

    private void CreatePurchasables()
    {
        foreach (Transform itemUI in cosmeticsPanel)
        {
            Transform item = itemUI.Find("Name");
            TextMeshProUGUI name = item?.GetComponent<TextMeshProUGUI>();
            if (name == null) continue;

            VendingMachineItem match = vendingMachineItems.Find(vm => vm.itemName == name.text);
            if (match != null)
            {
                Transform iconTransform = itemUI.Find("Icon");
                Image iconImage = iconTransform?.GetComponent<Image>();
                if (iconImage != null)
                {
                    match.itemImage = iconImage.sprite;
                    match.spawnPosition = iconImage.transform;
                }
            }
        }
    }

    [Serializable]
    private class Wrapper
    {
        public List<VendingMachineItem> vendingMachineItems;
    }

    [Serializable]
    private class VendingMachineItem
    {
        public string itemCode;
        public string itemName;
        public string itemDescription;
        public string itemEffect;
        public int stockDay = 0;

        [NonSerialized] public Sprite itemImage;
        [NonSerialized] public Transform spawnPosition;
    }
}