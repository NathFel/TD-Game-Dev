using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public enum GamePhase
    {
        DrawAndBuild,
        EnemyWave,
        Shop
    }

    [Header("Hand Settings")]
    public int maxHandSize = 10;
    public int startingHandCount = 5;
    public int handDrawPerRound = 2;

    [Header("Deck Settings")]
    public int maxDeckSize = 20;
    public List<Card> startingDeck = new List<Card>();

    [HideInInspector] public List<Card> currentDeck = new List<Card>();
    [HideInInspector] public List<Card> drawPile = new List<Card>();
    [HideInInspector] public List<Card> hand = new List<Card>();
    [HideInInspector] public List<Card> discardPile = new List<Card>();

    [Header("UI")]
    public Transform handPanel;
    public CardUI cardUIPrefab;

    [Header("Wave Settings")]
    public int maxWave = 5;

    private bool isPlacing = false;
    private CardUI placingCardUI = null;

    private int currentRound = 1; // Incremented before shop
    private int currentWave = 1;  // Incremented within round
    private GamePhase currentPhase;

    private bool firstDrawThisRound = true;

    void Start()
    {
        currentDeck.AddRange(startingDeck);
        ShuffleDrawPile();
        StartPhase(GamePhase.DrawAndBuild);
    }

    private void StartPhase(GamePhase phase)
    {
        currentPhase = phase;
        Debug.Log($"Starting Phase: {phase} | Round: {currentRound} | Wave: {currentWave}");

        switch (phase)
        {
            case GamePhase.DrawAndBuild:
                DrawHand();
                break;
            case GamePhase.Shop:
                ShopManager.Instance.OpenShop(OnShopClosed);
                break;
            case GamePhase.EnemyWave:
                // Enemy waves start via StartWaveButton
                break;
        }
    }

    private void ShuffleDrawPile()
    {
        drawPile.Clear();
        drawPile.AddRange(currentDeck);
        for (int i = 0; i < drawPile.Count; i++)
        {
            int rnd = Random.Range(0, drawPile.Count);
            Card temp = drawPile[rnd];
            drawPile[rnd] = drawPile[i];
            drawPile[i] = temp;
        }
    }

    public void DrawHand()
    {
        if (cardUIPrefab == null)
        {
            Debug.LogError("DeckManager: CardUIPrefab is not assigned!");
            return;
        }

        int drawCount = firstDrawThisRound ? startingHandCount : handDrawPerRound;
        firstDrawThisRound = false;

        drawCount = Mathf.Min(drawCount, maxHandSize - hand.Count);
        if (drawCount <= 0) return;

        for (int i = 0; i < drawCount; i++)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0)
                    break;

                drawPile.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDrawPile();
            }

            int idx = Random.Range(0, drawPile.Count);
            Card drawn = drawPile[idx];
            drawPile.RemoveAt(idx);
            hand.Add(drawn);

            CardUI cu = Instantiate(cardUIPrefab, handPanel);
            cu.Setup(drawn, this);
        }
    }

    public void RequestPlayCard(Card card, CardUI cardUI)
    {
        if (card == null || cardUI == null) return;

        if (card.cardType == Card.CardType.Tower && card.towerPrefab != null)
        {
            if (currentPhase != GamePhase.DrawAndBuild)
            {
                Debug.Log("Towers can only be placed during the Build phase.");
                return;
            }

            if (isPlacing)
            {
                Debug.Log("Already placing a tower.");
                return;
            }

            isPlacing = true;
            placingCardUI = cardUI;
            placingCardUI.SetInteractable(false);

            PlacementManager.Instance.StartPlacing(
                card.towerPrefab,
                onPlaced: () => OnCardPlaced(card),
                onCanceled: () => OnPlacementCanceled()
            );
        }
        else if (card.cardType == Card.CardType.Spell)
        {
            if (currentPhase == GamePhase.EnemyWave)
            {
                Debug.Log("Cast Spell card: " + card.cardName);
                OnCardPlaced(card);
            }
            else
            {
                Debug.Log("Spells can only be used during the Enemy Wave phase.");
            }
        }
        else if (card.cardType == Card.CardType.Upgrade)
        {
            if (currentPhase == GamePhase.DrawAndBuild)
            {
                Debug.Log("Apply Upgrade card: " + card.cardName);
                OnCardPlaced(card);
            }
            else
            {
                Debug.Log("Upgrades can only be used during the Build phase.");
            }
        }
    }

    private void OnCardPlaced(Card card)
    {
        if (hand.Contains(card))
            hand.Remove(card);

        discardPile.Add(card);

        if (placingCardUI != null)
            Destroy(placingCardUI.gameObject);

        isPlacing = false;
        placingCardUI = null;
    }

    private void OnPlacementCanceled()
    {
        if (placingCardUI != null)
            placingCardUI.SetInteractable(true);

        isPlacing = false;
        placingCardUI = null;
    }

    public void AddCardToDeck(Card card)
    {
        if (currentDeck.Count < maxDeckSize)
        {
            currentDeck.Add(card);
            Debug.Log("Added " + card.cardName + " to deck.");
        }
        else
        {
            Debug.Log("Deck is full! Cannot add " + card.cardName);
        }
    }

    public void StartWaveButton()
    {
        if (currentPhase == GamePhase.DrawAndBuild)
        {
            StartPhase(GamePhase.EnemyWave);
            EnemyWaveManager.Instance.StartWave(currentRound, currentWave, OnWaveComplete);
        }
    }

    private void OnWaveComplete()
    {
        currentWave++;

        if (currentWave > maxWave)
        {
            currentRound++;
            firstDrawThisRound = true;

            CollectAllCardsToDeck();

            StartPhase(GamePhase.Shop);

            currentWave = 1; // reset wave for new round
        }
        else
        {
            StartPhase(GamePhase.DrawAndBuild);
        }
    }

    private void CollectAllCardsToDeck()
    {
        foreach (var card in hand)
            if (!currentDeck.Contains(card))
                currentDeck.Add(card);
        hand.Clear();

        foreach (var card in discardPile)
            if (!currentDeck.Contains(card))
                currentDeck.Add(card);
        discardPile.Clear();

        foreach (var card in drawPile)
            if (!currentDeck.Contains(card))
                currentDeck.Add(card);
        drawPile.Clear();

        ShuffleDrawPile();
        Debug.Log("All cards collected to deck before shop. Total cards: " + currentDeck.Count);
    }

    private void OnShopClosed()
    {
        StartPhase(GamePhase.DrawAndBuild);
    }

    // ===== Optional: Debug / UI methods =====
    public void ShowCurrentDeck(Transform panel)
    {
        foreach (Transform child in panel) Destroy(child.gameObject);
        foreach (Card c in currentDeck)
        {
            CardUI cu = Instantiate(cardUIPrefab, panel);
            cu.Setup(c, null);
            cu.SetInteractable(false);
        }
    }

    public void ShowDiscardPile(Transform panel)
    {
        foreach (Transform child in panel) Destroy(child.gameObject);
        foreach (Card c in discardPile)
        {
            CardUI cu = Instantiate(cardUIPrefab, panel);
            cu.Setup(c, null);
            cu.SetInteractable(false);
        }
    }

    public void ShowDrawPile(Transform panel)
    {
        foreach (Transform child in panel) Destroy(child.gameObject);
        foreach (Card c in drawPile)
        {
            CardUI cu = Instantiate(cardUIPrefab, panel);
            cu.Setup(c, null);
            cu.SetInteractable(false);
        }
    }
}
