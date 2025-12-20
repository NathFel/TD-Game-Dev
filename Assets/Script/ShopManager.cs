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
    public Transform shopContent;
    public int cardsToShow = 7;
    public float costDistanceFromCard = 20f;
    public Button exitShopButton;

    [Header("Shop Settings")]
    public List<Card> availableCards = new List<Card>();

    [Header("Destroy Card UI")]
    public Button destroyCardButton;
    public GameObject destroyPanel;
    public Transform destroyContent;
    public Button confirmDestroyButton;
    public Button cancelDestroyButton;
    public TextMeshProUGUI destroyCostText;
    public int destroyCardCost = 30;

    [Header("Destroy Limits")]
    public int maxDestroyPerShop = 1;
    private int destroyUsedThisShop = 0;

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
            Debug.LogError("No DeckManager found!");

        exitShopButton.onClick.AddListener(CloseShop);
        destroyCardButton.onClick.AddListener(OpenDestroyMenu);
        confirmDestroyButton.onClick.AddListener(ConfirmDestroyCard);
    }

    // ===================== SHOP =====================

    public void OpenShop(Action onClose)
    {
        onShopClosed = onClose;

        shopPanel.SetActive(true);
        destroyPanel.SetActive(false);

        destroyUsedThisShop = 0; // reset for this shop
        destroyCardButton.interactable = true; // enable button

        PopulateShop();
    }

    private void PopulateShop()
    {
        foreach (Transform child in shopContent)
            Destroy(child.gameObject);

        SpawnCards(CardUI.Rarity.Common, 4);
        SpawnCards(CardUI.Rarity.Rare, 2);
        SpawnCards(CardUI.Rarity.Legendary, 1);
    }

    private void SpawnCards(CardUI.Rarity rarity, int count)
    {
        List<Card> pool = availableCards.FindAll(c => c.rarity == rarity);
        if (pool.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            Card card = pool[UnityEngine.Random.Range(0, pool.Count)];

            CardUI cu = Instantiate(deckManager.cardUIPrefab, shopContent);
            cu.Setup(card, null);
            cu.SetInteractable(true);

            Button btn = cu.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => BuyCard(card, cu.gameObject));

            AddCostText(cu.transform, card.cost);
        }
    }

    private void BuyCard(Card card, GameObject cardGO)
    {
        if (PlayerStats.Money < card.cost) return;
        if (deckManager.currentDeck.Count >= deckManager.maxDeckSize) return;

        PlayerStats.Money -= card.cost;
        deckManager.AddCardToDeck(card);

        Destroy(cardGO);
    }

    private void AddCostText(Transform parent, int cost)
    {
        GameObject costGO = new GameObject("CostText", typeof(RectTransform));
        costGO.transform.SetParent(parent);
        costGO.transform.localScale = Vector3.one;

        TextMeshProUGUI text = costGO.AddComponent<TextMeshProUGUI>();
        text.text = $"Cost: {cost}";
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 50;

        RectTransform rt = costGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -costDistanceFromCard);
        rt.sizeDelta = new Vector2(0, 20);
    }

    private void OpenDestroyMenu()
    {
        if (destroyUsedThisShop >= maxDestroyPerShop)
        {
            Debug.Log("Destroy card can only be used once per shop!");
            return;
        }

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

        Debug.Log("Destroyed card: " + selectedDestroyCard.cardName);

        destroyUsedThisShop++;

        // Refresh UI
        PopulateDestroyList();
        destroyPanel.SetActive(false);
    }

    public void CancelDestroy(){

        destroyPanel.SetActive(false);
        selectedDestroyCard = null; // reset selection

    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        destroyPanel.SetActive(false);
        destroyUsedThisShop = 0;
        destroyCardButton.interactable = true;
        onShopClosed?.Invoke();
    }
}
