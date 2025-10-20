using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

     public List<Card> currentDeck = new List<Card>();
     public List<Card> drawPile = new List<Card>();
     public List<Card> hand = new List<Card>();
     public List<Card> discardPile = new List<Card>();

    [Header("UI")]
    public TMP_Text phaseText;
    public Transform handPanel;
    public CardUI cardUIPrefab;
    public float shopSpawnChance = 0.5f;

    [Header("Deck UI Buttons")]
    public Button showDeckButton;
    public Button showDiscardButton;
    public DeckListUI deckListUI;

    [Header("Wave Settings")]
    public int maxWave = 5;

    private bool isPlacing = false;
    private CardUI placingCardUI = null;

    [HideInInspector]public int currentRound = 1;
    [HideInInspector]public int currentWave = 1;
    private GamePhase currentPhase;
    private bool firstDrawThisRound = true;
    private int selectedCardIndex = -1;
    [HideInInspector] public int hoveredIndex = -1;

    public static DeckManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        currentDeck.AddRange(startingDeck);
        ShuffleDrawPile();
        StartPhase(GamePhase.DrawAndBuild);

         if (showDeckButton != null)
            showDeckButton.onClick.AddListener(() => deckListUI.ShowDeck(currentDeck, "Current Deck"));

        if (showDiscardButton != null)
            showDiscardButton.onClick.AddListener(() => deckListUI.ShowDeck(discardPile, "Discard Pile"));
    }

    void Update(){
        UpdateHandLayout();

        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    public void SetHandActive(bool active)
    {
        if (handPanel != null)
            handPanel.gameObject.SetActive(active);
    }

    private void StartPhase(GamePhase phase)
    {
        if (isPlacing)
        {
            isPlacing = false;
            placingCardUI?.SetInteractable(true);
            placingCardUI = null;

            // Also cancel managers to clean up any ghost placement
            PlacementManager.Instance?.CancelPlacement();
            SpellPlacementManager.Instance?.CancelPlacement();
        }
        
        DeselectCard();
        
        currentPhase = phase;
        Debug.Log($"Starting Phase: {phase} | Round: {currentRound} | Wave: {currentWave}");
        UpdatePhaseText($"{phase} Phase | Round: {currentRound} | Wave: {currentWave}");

        switch (phase)
        {
            case GamePhase.DrawAndBuild:
                DiscardHand();
                DrawHand();
                break;
            case GamePhase.Shop:
                if (ShopManager.Instance != null)      
                    SetHandActive(false);
                    ShopManager.Instance.OpenShop(OnShopClosed);
                break;
        }
    }

    private void UpdatePhaseText(string message)
    {
        if (phaseText != null)
            phaseText.text = message;
        else
            Debug.LogWarning("TMP Text not assigned in the Inspector!");
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

    public void RemoveCardFromDeck(Card card)
    {
        if (currentDeck.Contains(card))
        {
            currentDeck.Remove(card);
            Debug.Log("Removed " + card.cardName + " from deck.");
        }
        else
        {
            Debug.LogWarning("Tried to remove a card that isn’t in the deck: " + card.cardName);
        }
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
            {
                bool isElitePhase = (currentRound % 5 == 0 && currentRound % 10 != 0 && currentWave == maxWave);
                bool isBossPhase = (currentRound % 10 == 0 && currentWave == maxWave);

                EnemyWaveManager.Instance.StartWave(currentRound, currentWave, isElitePhase, isBossPhase, OnWaveComplete);
            }
        }
    }

    private void OnWaveComplete()
    {
        // Cleanup hand + board
        DiscardHand();
        ClearBoard();
        TurretInfoUI.Instance?.Hide();

        currentWave++;

        if (currentWave > maxWave)
        {
            currentRound++;
            firstDrawThisRound = true;
            CollectAllCardsToDeck();
            
            int roundReward = currentRound * 200; 
            ShowReward(roundReward, afterReward: () =>
            {
                currentWave = 1;

                // --- Determine if shop should appear ---
                if (Random.value <= shopSpawnChance && ShopManager.Instance != null)
                {
                    StartPhase(GamePhase.Shop);
                }
                else
                {
                    // If no shop, go straight to DrawAndBuild phase
                    StartPhase(GamePhase.DrawAndBuild);
                }
            });
        }
        else 
        {
            StartPhase(GamePhase.DrawAndBuild);
        } 
    }

    private void ShowReward(int waveMoney, System.Action afterReward)
    {
        if (RewardUIManager.Instance != null)
        {
            RewardUIManager.Instance.ShowReward(waveMoney, afterReward);
        }
        else
        {
            afterReward?.Invoke();
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
        SetHandActive(true);
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