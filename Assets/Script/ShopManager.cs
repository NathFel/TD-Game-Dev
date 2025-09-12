using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("UI")]
    public GameObject shopPanel;
    public TextMeshProUGUI moneyText; 
    public Transform shopContent; // container for cards
    public int cardsToShow = 4;
    public float costDistanceFromCard = 20f; // adjustable distance
    public Button exitShopButton; // assign in Inspector

    [Header("Shop Settings")]
    public List<Card> availableCards = new List<Card>();

    private Action onShopClosed;
    private DeckManager deckManager;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one ShopManager in scene!");
            return;
        }
        Instance = this;

        deckManager = FindObjectOfType<DeckManager>();
        if (deckManager == null)
            Debug.LogError("No DeckManager found in scene!");

        // Assign exit button listener
        if (exitShopButton != null)
            exitShopButton.onClick.AddListener(CloseShop);
    }

    public void OpenShop(Action onClose)
    {
        onShopClosed = onClose;
        shopPanel.SetActive(true);
        UpdateMoneyUI();
        PopulateShop();
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"Money: {PlayerStats.Money}";
    }

    private void PopulateShop()
    {
        // Clear previous shop cards
        foreach (Transform child in shopContent)
            Destroy(child.gameObject);

        List<Card> tempPool = new List<Card>(availableCards);

        for (int i = 0; i < cardsToShow && tempPool.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, tempPool.Count);
            Card card = tempPool[idx];
            tempPool.RemoveAt(idx);

            // Instantiate CardUI like hand
            CardUI cu = Instantiate(deckManager.cardUIPrefab, shopContent);
            cu.Setup(card, null); // no DeckManager reference needed
            cu.SetInteractable(true); // make sure button is clickable

            // Remove any default onClick (from CardUI)
            Button btn = cu.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => BuyCard(card, cu.gameObject));
            }

            // Add TMP cost text below card
            GameObject costGO = new GameObject("CostText", typeof(RectTransform));
            costGO.transform.SetParent(cu.transform);
            costGO.transform.localScale = Vector3.one;

            TextMeshProUGUI costText = costGO.AddComponent<TextMeshProUGUI>();
            costText.text = $"Cost: {card.cost}";
            costText.alignment = TextAlignmentOptions.Center;
            costText.fontSize = 18;

            RectTransform rt = costGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -costDistanceFromCard);
            rt.sizeDelta = new Vector2(0, 20);
        }
    }

    private void BuyCard(Card card, GameObject cardGO)
    {
        if (PlayerStats.Money < card.cost)
        {
            Debug.Log("Not enough money for " + card.cardName);
            return;
        }

        // Check if adding the card would exceed maxDeckSize
        if (deckManager.currentDeck.Count >= deckManager.maxDeckSize)
        {
            Debug.Log("Deck is full! Cannot buy " + card.cardName);
            return;
        }

        // Deduct money and add card to currentDeck
        PlayerStats.Money -= card.cost;
        deckManager.AddCardToDeck(card);

        // Remove the card from shop UI
        Destroy(cardGO);

        // Update money display
        UpdateMoneyUI();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        onShopClosed?.Invoke();
    }
}
