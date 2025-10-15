using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class RewardUIManager : MonoBehaviour
{
    public static RewardUIManager Instance;

    [Header("UI References")]
    public GameObject rewardPanel;
    public Transform cardContainer;
    public CardUI cardUIPrefab;
    public Button skipButton;

    [Header("Reward Settings")]
    public List<Card> rewardPool = new List<Card>(); // all possible rewards
    public int cardsToOffer = 3;
    public float costDistanceFromCard = 20f;

    [Header("Rarity Settings")]
    public int pityThreshold = 4; // Legendary guaranteed every 4 rounds
    [HideInInspector]public int currentRound = 0;
    private bool legendaryGivenThisCycle = false;

    // Weight multipliers per rarity (adjust freely)
    private readonly Dictionary<CardUI.Rarity, float> rarityWeights = new Dictionary<CardUI.Rarity, float>
    {
        { CardUI.Rarity.Common, 70f },
        { CardUI.Rarity.Rare, 20f },
        { CardUI.Rarity.Epic, 8f },
        { CardUI.Rarity.Legendary, 2f }
    };

    private System.Action onRewardComplete;
    private DeckManager deckManager;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one RewardUIManager in scene!");
            return;
        }
        Instance = this;

        deckManager = FindObjectOfType<DeckManager>();
        if (deckManager == null)
            Debug.LogError("No DeckManager found in scene!");

        // Assign skip button listener
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipReward);
    }

    public void ShowReward(int waveMoney, System.Action onComplete)
    {
        onRewardComplete = onComplete;
        rewardPanel.SetActive(true);

        // Clear old UI
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        // Determine if pity should trigger
        currentRound++;
        bool pityTrigger = !legendaryGivenThisCycle && currentRound >= pityThreshold;

        // Select cards to offer
        List<Card> selectedRewards = GenerateRewardCards(pityTrigger);

        if (pityTrigger)
        {
            legendaryGivenThisCycle = true;
            currentRound = 0; // Reset pity after guaranteed legendary
        }

        // Instantiate UI
        foreach (Card card in selectedRewards)
        {
            CardUI cu = Instantiate(cardUIPrefab, cardContainer);
            cu.Setup(card, null);
            cu.SetInteractable(true);

            Button btn = cu.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SelectCard(card, cu.gameObject));
            }

            // TMP label below the card
            GameObject costGO = new GameObject("RewardLabel", typeof(RectTransform));
            costGO.transform.SetParent(cu.transform);
            costGO.transform.localScale = Vector3.one;

            TextMeshProUGUI costText = costGO.AddComponent<TextMeshProUGUI>();
            costText.text = "Add to Deck";
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

    private List<Card> GenerateRewardCards(bool forceLegendary)
    {
        List<Card> available = new List<Card>(rewardPool);
        List<Card> selected = new List<Card>();

        // Filter by rarity
        List<Card> legendaries = available.Where(c => c.rarity == CardUI.Rarity.Legendary).ToList();

        if (forceLegendary && legendaries.Count > 0)
        {
            // Guarantee at least one legendary
            Card legendaryCard = legendaries[Random.Range(0, legendaries.Count)];
            selected.Add(legendaryCard);
            available.Remove(legendaryCard);
        }

        // Fill remaining slots with weighted random
        while (selected.Count < cardsToOffer && available.Count > 0)
        {
            Card chosen = GetWeightedRandomCard(available);
            selected.Add(chosen);
            available.Remove(chosen);
        }

        return selected;
    }

    private Card GetWeightedRandomCard(List<Card> pool)
    {
        float totalWeight = pool.Sum(c => rarityWeights[c.rarity]);
        float roll = Random.Range(0, totalWeight);
        float cumulative = 0;

        foreach (var card in pool)
        {
            cumulative += rarityWeights[card.rarity];
            if (roll <= cumulative)
                return card;
        }

        return pool[0];
    }

    private void SelectCard(Card card, GameObject cardGO)
    {
        if (deckManager == null)
        {
            Debug.LogError("DeckManager not found!");
            return;
        }

        deckManager.AddCardToDeck(card);
        Debug.Log($"Added {card.cardName} ({card.rarity}) to deck!");
        CloseReward();
    }

    private void SkipReward()
    {
        Debug.Log("Player skipped reward.");
        CloseReward();
    }

    private void CloseReward()
    {
        rewardPanel.SetActive(false);
        onRewardComplete?.Invoke();
    }
}