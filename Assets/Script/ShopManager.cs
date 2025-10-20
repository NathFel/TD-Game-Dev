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
    public Transform shopContent;
    public int cardsToShow = 4;
    public float costDistanceFromCard = 20f;
    public Button exitShopButton;

    [Header("Shop Settings")]
    public List<Card> availableCards = new List<Card>();

    [Header("Destroy Card UI")]
    public Button destroyCardButton;
    public GameObject destroyPanel;
    public Transform destroyContent;
    public Button confirmDestroyButton;
    public TextMeshProUGUI destroyCostText;
    public int destroyCardCost = 30;

    private Action onShopClosed;
    private DeckManager deckManager;
    private Card selectedDestroyCard = null;

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

        // Assign listeners
        if (exitShopButton != null)
            exitShopButton.onClick.AddListener(CloseShop);

        if (destroyCardButton != null)
            destroyCardButton.onClick.AddListener(OpenDestroyMenu);

        if (confirmDestroyButton != null)
            confirmDestroyButton.onClick.AddListener(ConfirmDestroyCard);
    }

    public void OpenShop(Action onClose)
    {
        onShopClosed = onClose;
        shopPanel.SetActive(true);
        destroyPanel.SetActive(false);
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
        foreach (Transform child in shopContent)
            Destroy(child.gameObject);

        List<Card> tempPool = new List<Card>(availableCards);

        for (int i = 0; i < cardsToShow && tempPool.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, tempPool.Count);
            Card card = tempPool[idx];
            tempPool.RemoveAt(idx);

            // Instantiate Card UI
            CardUI cu = Instantiate(deckManager.cardUIPrefab, shopContent);
            cu.Setup(card, null);
            cu.SetInteractable(true);

            // Add BuyCard listener
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

        if (deckManager.currentDeck.Count >= deckManager.maxDeckSize)
        {
            Debug.Log("Deck is full! Cannot buy " + card.cardName);
            return;
        }

        // Deduct money
        PlayerStats.Money -= card.cost;
        deckManager.AddCardToDeck(card);
        UpdateMoneyUI();

        // Remove the card from shop UI
        Destroy(cardGO);
    }

    private void OpenDestroyMenu()
    {
        if (PlayerStats.Money < destroyCardCost)
        {
            Debug.Log("Not enough money to destroy a card!");
            return;
        }

        destroyPanel.SetActive(true);
        PopulateDestroyList();
        UpdateDestroyCostUI();
    }

    private void UpdateDestroyCostUI()
    {
        if (destroyCostText != null)
            destroyCostText.text = $"Cost: {destroyCardCost}";
    }

    private void PopulateDestroyList()
    {
        foreach (Transform child in destroyContent)
            Destroy(child.gameObject);

        foreach (Card card in deckManager.currentDeck)
        {
            CardUI cu = Instantiate(deckManager.cardUIPrefab, destroyContent);
            cu.Setup(card, null);
            cu.SetInteractable(true);

            Button btn = cu.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SelectCardToDestroy(card, cu));
            }
        }

        selectedDestroyCard = null;
    }

    private void SelectCardToDestroy(Card card, CardUI cu)
    {
        selectedDestroyCard = card;

        foreach (Transform child in destroyContent)
        {
            Image img = child.GetComponent<Image>();
            if (img != null)
                img.color = Color.white;
        }

        Image selectedImg = cu.GetComponent<Image>();
        if (selectedImg != null)
            selectedImg.color = Color.red;
    }

    private void ConfirmDestroyCard()
    {
        if (selectedDestroyCard == null)
        {
            Debug.Log("No card selected to destroy.");
            return;
        }

        if (PlayerStats.Money < destroyCardCost)
        {
            Debug.Log("Not enough money to destroy a card!");
            return;
        }

        // Deduct cost and remove card
        PlayerStats.Money -= destroyCardCost;
        deckManager.RemoveCardFromDeck(selectedDestroyCard);
        UpdateMoneyUI();

        Debug.Log("Destroyed card: " + selectedDestroyCard.cardName);

        // Refresh UI
        PopulateDestroyList();
        destroyPanel.SetActive(false);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        destroyPanel.SetActive(false);
        onShopClosed?.Invoke();
    }
}
