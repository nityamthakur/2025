using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VendingMachine : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayPanel;
    [SerializeField] private TextMeshProUGUI nameText, descriptionText, effectText;
    [SerializeField] private Transform cosmeticsPanel, spawnLayer;
    [SerializeField] private Transform vendingMachineSlots;
    private GameManager gameManager;
    private ShopScene shopScene;

    private List<VendingMachineItem> vendingMachineItems;
    private string itemCode;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        shopScene = FindFirstObjectByType<ShopScene>();
        itemCode = "";

        LoadJsonFromFile();
        //CreatePurchasables();
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

    private void ItemCheck()
    {
        VendingMachineItem item = vendingMachineItems.Find(i => i.itemCode == itemCode);
        if (item == null)
        {
            //Debug.Log("Item not found with code: " + itemCode);
            return;
        }

        var cosmetic = Array.Find(shopScene.cosmeticItems, c => c.displayName == item.itemName);
        if (cosmetic == null) return;

        Transform cosmeticsPanel = shopScene.currentShopScreen.transform.Find("CosmeticsPanel");
        Transform icon = null;

        foreach (Transform itemInPanel in cosmeticsPanel)
        {
            TextMeshProUGUI nameField = itemInPanel.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameField != null && nameField.text == item.itemName)
            {
                icon = itemInPanel.Find("Icon");
                break;
            }
        }
        if (icon == null)
        {
            Debug.LogWarning("Could not find icon for item: " + item.itemName);
            return;
        }

        if (gameManager.gameData.IsCosmeticPurchased(cosmetic.id)) return;
        if (gameManager.gameData.GetCurrentMoney() < cosmetic.price) return;


        ItemFall(cosmetic, icon.position);
    }

    private void ItemFall(ShopScene.CosmeticShopItem cosmetic, Vector3 iconPosition)
    {
        Vector3 pos = iconPosition;
        pos += new Vector3(5f, 2f, 0f);

        EventManager.PurchaseCosmeticById?.Invoke(cosmetic.id);

        GameObject entry = Instantiate(shopScene.cosmeticShopEntryPrefab, spawnLayer);
        entry.transform.Find("Icon").GetComponent<Image>().sprite = cosmetic.icon;
        entry.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = "";
        entry.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = "";
        entry.transform.Find("BuyButton").gameObject.SetActive(false);

        entry.transform.position = pos;
        Rigidbody2D rb = entry.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;

        StartCoroutine(DelaySound(1f));
        Destroy(entry, 2f);
    }

    private void VendingMachineItemFall(string itemName)
    {
        var cosmetic = Array.Find(shopScene.cosmeticItems, c => c.displayName == itemName);
        if (cosmetic == null)
        {
            Debug.LogWarning("No cosmetic found with name: " + itemName);
            return;
        }

        Transform cosmeticsPanel = shopScene.currentShopScreen.transform.Find("CosmeticsPanel");
        Transform icon = null;

        foreach (Transform itemInPanel in cosmeticsPanel)
        {
            TextMeshProUGUI nameField = itemInPanel.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameField != null && nameField.text == itemName)
            {
                icon = itemInPanel.Find("Icon");
                break;
            }
        }

        if (icon == null)
        {
            Debug.LogWarning("Could not find icon for item: " + itemName);
            return;
        }

        ItemFall(cosmetic, icon.position);
    }

    
    void OnEnable()
    {
        EventManager.VendingMachineItemFall += VendingMachineItemFall;
    }

    void OnDisable()
    {
        EventManager.VendingMachineItemFall -= VendingMachineItemFall;
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

    public void CreatePurchasables()
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
        public int itemCost;
        public int stockDay = 0;

        [NonSerialized] public Sprite itemImage;
        [NonSerialized] public Transform spawnPosition;
    }
}