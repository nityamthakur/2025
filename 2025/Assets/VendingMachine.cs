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
    //[SerializeField] private GameObject machineButtons;
    [SerializeField] private Transform cosmeticsPanel, spawnLayer;
    [SerializeField] private GameObject fallingImage;
    private GameManager gameManager;
    private List<VendingMachineItem> vendingMachineItems;
    private string itemCode = "";

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        LoadJsonFromFile();
        CreatePurchasables();
        CancelButtonPanel();
    }

    public void ConfirmPurchase()
    {
        ItemFall();
        CancelButtonPanel();
    }
    public void CancelButtonPanel()
    {
        itemCode = "";
        UpdateDisplayPanel();
    }
    public void InputItemCode(string code)
    {
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
        if (item == null)
            return;

        Debug.Log("Spawning object");
        GameObject fallingObject = Instantiate(fallingImage, spawnLayer);
        fallingObject.SetActive(true);
        fallingObject.name = "FallingItem_" + item.itemName;
        fallingObject.transform.position = item.spawnWorldPosition;

        Image sr = fallingObject.GetComponent<Image>();
        if (sr != null)
        {
            sr.sprite = item.itemImage;
        }

        //Destroy(fallingObject, 2f);
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
            GameObject clone = Instantiate(itemUI.gameObject, itemUI.parent);
            clone.name = "CLONE_" + itemUI.name;
            //clone.SetSiblingIndex(itemUI.GetSiblingIndex() + 1); // Optional, put it right after the original
        }


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
                    match.spawnWorldPosition = iconImage.transform.position;
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
        [NonSerialized] public Vector3 spawnWorldPosition;

        public IEnumerator FallAndDestroy()
        {
            yield return new WaitForSeconds(1f);
            //Destroy(this.gameObject);
        }
    }
}
