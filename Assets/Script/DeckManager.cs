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
    public float cardSpacing = 200f;
    public float curveHeight = 100f;
    public float curveStrength = 1f;
    public float cardAngle = 10f;

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

    private int currentRound = 1;
    private int currentWave = 1;
    private GamePhase currentPhase;
    private bool firstDrawThisRound = true;
    private int selectedCardIndex = -1;
    [HideInInspector] public int hoveredIndex = -1;

    void Start()
    {
        currentDeck.AddRange(startingDeck);
        ShuffleDrawPile();
        StartPhase(GamePhase.DrawAndBuild);
    }

    void Update(){
        UpdateHandLayout();

        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    private void StartPhase(GamePhase phase)
    {
        currentPhase = phase;
        Debug.Log($"Starting Phase: {phase} | Round: {currentRound} | Wave: {currentWave}");

        switch (phase)
        {
            case GamePhase.DrawAndBuild:
                DiscardHand();
                DrawHand();
                break;
            case GamePhase.Shop:
                if (ShopManager.Instance != null)
                    ShopManager.Instance.OpenShop(OnShopClosed);
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
                if (discardPile.Count == 0) break;

                drawPile.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDrawPile();
            }

            int idx = Random.Range(0, drawPile.Count);
            Card drawn = drawPile[idx];
            drawPile.RemoveAt(idx);
            hand.Add(drawn);

            CardUI cu = Instantiate(cardUIPrefab, handPanel);
            float xOffset = 1000f; // adjust as needed
            Vector3 pos = cu.transform.localPosition; // get current position
            pos.x += xOffset; // add offset
            cu.transform.localPosition = pos; // apply new position

            cu.Setup(drawn, this);
        }
    }

    private void DiscardHand()
    {
        foreach (var card in hand)
            discardPile.Add(card);
        hand.Clear();

        foreach (Transform child in handPanel)
            Destroy(child.gameObject);
    }

    public void SelectCard(int index)
    {
        if (selectedCardIndex == index)
            selectedCardIndex = -1;
        else
            selectedCardIndex = index;

        // Cancel any active placement when selecting a card
        if (PlacementManager.Instance != null) PlacementManager.Instance.CancelPlacement();
        if (SpellPlacementManager.Instance != null) SpellPlacementManager.Instance.CancelPlacement();
    }

    public void DeselectCard()
    {
        selectedCardIndex = -1;
    }

    public void HandleRightClick()
    {
        PlacementManager.Instance?.CancelPlacement();
        SpellPlacementManager.Instance?.CancelPlacement();
        DeselectCard();
    }
    
    public void UpdateHandLayout(int draggingIndex = -1)
    {
        int cardCount = handPanel.childCount;
        if (cardCount == 0) return;

        float midIndex = (cardCount - 1) / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            RectTransform rect = handPanel.GetChild(i).GetComponent<RectTransform>();
            if (rect == null) continue;

            CardUI cu = rect.GetComponent<CardUI>();

            // --- Use hierarchy index for hovered/selected ---
            bool isHovered = (hoveredIndex == i);
            bool isSelected = (selectedCardIndex == i);

            // --- Base X position ---
            float xPos = (i - midIndex) * cardSpacing;

            // --- Arc using parabola ---
            float distanceFromCenter = (i - midIndex);
            float curveOffset = -curveStrength * Mathf.Pow(distanceFromCenter, 2) + curveHeight;

            // --- Hover neighbors move away ---
            if (hoveredIndex >= 0 && i != hoveredIndex)
            {
                float direction = i < hoveredIndex ? -1f : 1f;
                xPos += direction * 30f;
            }

            if (isSelected) curveOffset += curveHeight * 0.6f;
            else if (isHovered) curveOffset += curveHeight * 0.3f;

            Vector3 targetPos = new Vector3(xPos, curveOffset, 0f);

            // --- Scale ---
            float targetScale = 1f;
            if (cu != null)
            {
                if (isSelected) targetScale = cu.hoverScale * 1.2f;
                else if (isHovered) targetScale = cu.hoverScale;
                else targetScale = cu.normalScale;
            }

            float moveSpeed = 800f;
            rect.localPosition = Vector3.MoveTowards(rect.localPosition, targetPos, moveSpeed * Time.deltaTime);
            rect.localScale = Vector3.MoveTowards(rect.localScale, Vector3.one * targetScale, moveSpeed * Time.deltaTime);

            // --- Rotation ---
            float angle = (i - midIndex) * -cardAngle * curveStrength;
            if (isHovered || isSelected) angle = 0;
            rect.localRotation = Quaternion.RotateTowards(rect.localRotation, Quaternion.Euler(0, 0, angle), moveSpeed * 10f * Time.deltaTime);

            // --- Canvas sorting ---
            Canvas cardCanvas = rect.GetComponent<Canvas>();
            if (cardCanvas != null)
            {
                cardCanvas.overrideSorting = true;
                cardCanvas.sortingOrder = isSelected ? 100 : 0;
            }

            // --- Update CardUI handIndex dynamically ---
            if (cu != null) cu.handIndex = i;
        }
    }


    // === Play, discard, shop, etc. (same as before) ===
    public void RequestPlayCard(Card card, CardUI cardUI)
    {
        if (card == null || cardUI == null) return;

        if (card.cardType == Card.CardType.Tower && card.towerPrefab != null)
        {
            if (currentPhase != GamePhase.DrawAndBuild)
            {
                Debug.Log("Towers can only be placed during the Build phase.");
                DeselectCard();
                return;
            }

            if (isPlacing) return;

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
            if (currentPhase != GamePhase.EnemyWave)
            {
                Debug.Log("Spells can only be used during Enemy Wave phase.");
                DeselectCard();
                return;
            }

            if (isPlacing) return;

            isPlacing = true;
            placingCardUI = cardUI;
            placingCardUI.SetInteractable(false);

            SpellPlacementManager.Instance.StartPlacingSpell(
                card,
                onPlaced: () => OnCardPlaced(card),
                onCanceled: () => OnPlacementCanceled()
            );
        }
        else if (card.cardType == Card.CardType.Upgrade)
        {
            if (currentPhase == GamePhase.DrawAndBuild)
                OnCardPlaced(card);
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

        DeselectCard();
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
            if (EnemyWaveManager.Instance != null)
                EnemyWaveManager.Instance.StartWave(currentRound, currentWave, OnWaveComplete);
        }
    }

    private void OnWaveComplete()
    {
        // Cleanup hand + board
        DiscardHand();
        ClearBoard();

        currentWave++;

        if (currentWave > maxWave)
        {
            currentRound++;
            firstDrawThisRound = true;

            CollectAllCardsToDeck();
            StartPhase(GamePhase.Shop);

            currentWave = 1; // reset for next round
        }
        else
        {
            StartPhase(GamePhase.DrawAndBuild);
        }
    }

    private void ClearBoard()
    {
        Node[] nodes = FindObjectsOfType<Node>();
        foreach (Node node in nodes)
            node?.ClearNode();
    }

    private void CollectAllCardsToDeck()
    {
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
        // After shop: reshuffle, then draw fresh hand for Wave 1
        ShuffleDrawPile();
        StartPhase(GamePhase.DrawAndBuild);
    }

    // ===== Debug / UI =====
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
