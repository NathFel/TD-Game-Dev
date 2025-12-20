using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Card UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image rarityBackground;
    [SerializeField] private TMP_Text manaCostText;

    // --- FITUR BARU: Slider Transparansi ---
    [Range(0f, 1f)] 
    [SerializeField] private float cardAlpha = 0.85f; // Default agak transparan (85%)

    private Button button;
    private DeckManager deckManager;

    public Card CardData { get; private set; }

    // --- Animation & Layout ---
    private Vector3 targetPos;
    [HideInInspector] public float hoverScale = 1.05f;
    [HideInInspector] public float normalScale = 1f;
    [HideInInspector] public int handIndex;
    private Canvas canvas;
    private RectTransform rectTransform;

    // --- Rarity Enum ---
    public enum Rarity { Common, Rare, Legendary }

    // --- Display Modes ---
    public enum CardDisplayMode
    {
        Hand,
        DeckView,
        DiscardView,
        Shop
    }

    [HideInInspector] public CardDisplayMode currentMode = CardDisplayMode.Hand;

    // --- Buy Button (for shop mode) ---
    private Button buyButton;

    private void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // Try to find a buy button in children (optional)
        Transform buyButtonTransform = transform.Find("BuyButton");
        if (buyButtonTransform != null)
            buyButton = buyButtonTransform.GetComponent<Button>();
    }

    public void SetTargetLocalPosition(Vector3 pos)
    {
        targetPos = pos;
    }

    public void SetInteractable(bool value)
    {
        if (button != null)
            button.interactable = value;
    }

    // ===== Hover Handling =====
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentMode == CardDisplayMode.Hand && deckManager != null)
            deckManager.hoveredIndex = handIndex;

        // Show tooltip on hover
        TDGameDev.UI.TooltipManager.Instance?.Show(CardData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentMode == CardDisplayMode.Hand && deckManager != null)
            deckManager.hoveredIndex = -1;

        // Hide tooltip when leaving
        TDGameDev.UI.TooltipManager.Instance?.Hide();
    }

    // ===== Setup Card =====
    public void Setup(Card card, DeckManager manager)
    {
        CardData = card;
        deckManager = manager;

        if (nameText != null) nameText.text = card.cardName;
        if (artworkImage != null) artworkImage.sprite = card.artwork;
        if (descriptionText != null) descriptionText.text = card.description;

        if (manaCostText != null)
            manaCostText.text = card.manaCost.ToString();

        // Set rarity color (Script akan otomatis baca settingan Alpha kamu disini)
        if (rarityBackground != null)
            SetRarity(card.rarity);

        // Default mode is hand
        currentMode = CardDisplayMode.Hand;

        // Setup click event (for hand cards only)
        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() =>
            {
                if (currentMode == CardDisplayMode.Hand)
                {
                    OnCardClicked();
                    deckManager?.RequestPlayCard(CardData, this);
                }
            });
        }

        // Hide buy button by default
        if (buyButton != null)
            buyButton.gameObject.SetActive(false);
    }

    private void OnCardClicked()
    {
        if (deckManager == null) return;

        // Cancel any placement
        PlacementManager.Instance?.CancelPlacement();
        SpellPlacementManager.Instance?.CancelPlacement();

        int index = transform.GetSiblingIndex();
        deckManager.SelectCard(index);
    }

    // ===== Rarity System (UPDATED) =====
    public void SetRarity(Rarity rarity)
    {
        string hexColor = "#808080"; // Default: Common (gray)
        switch (rarity)
        {
            case Rarity.Common:
                hexColor = "#808080"; // Gray
                break;
            case Rarity.Rare:
                hexColor = "#0e56db"; // Blue (FF dibuang biar bisa diatur alpha-nya)
                break;
            case Rarity.Legendary:
                hexColor = "#F1C40F"; // Gold
                break;
        }

        if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
        {
            // INI PERBAIKANNYA: Paksa alpha ikut settingan slider
            color.a = cardAlpha; 
            rarityBackground.color = color;
        }
    }

    // ===== Shop / Buy Mode =====
    public void EnableBuyMode(Action onBuy)
    {
        currentMode = CardDisplayMode.Shop;

        // Disable main button (hand click)
        SetInteractable(false);

        // Enable buy button
        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(true);
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuy?.Invoke());
        }
    }

    // ===== Deck Display Support =====
    public void SetDisplayMode(CardDisplayMode mode)
    {
        currentMode = mode;
        SetInteractable(mode == CardDisplayMode.Hand);

        // Hide buy button unless in shop
        if (buyButton != null)
            buyButton.gameObject.SetActive(mode == CardDisplayMode.Shop);
    }
}