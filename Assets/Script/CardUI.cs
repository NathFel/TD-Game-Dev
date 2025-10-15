using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text descriptionText;

    [SerializeField] private Image rarityBackground; // New field for rarity background

    private Button button; // root button
    private DeckManager deckManager;

    public Card CardData { get; private set; }

    // --- Animation settings ---
    private Vector3 targetPos;
    [HideInInspector] public float hoverScale = 1.05f;
    [HideInInspector] public float normalScale = 1f;
    [HideInInspector] public int handIndex;
    private Canvas canvas;
    private RectTransform rectTransform;

    // --- Rarity Enum ---
    public enum Rarity { Common, Rare, Epic, Legendary }

    private void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void SetTargetLocalPosition(Vector3 pos)
    {
        targetPos = pos;
    }

    public void SetInteractable(bool value)
    {
        if (button != null) button.interactable = value;
    }

    // ===== Hover pop-up =====
    public void OnPointerEnter(PointerEventData eventData)
    {
        deckManager.hoveredIndex = handIndex; // handIndex updated dynamically
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        deckManager.hoveredIndex = -1;
    }

    // Click selects card
    private void OnCardClicked()
    {
        if (deckManager == null) return;

        // Cancel any active placement (tower or spell)
        PlacementManager.Instance?.CancelPlacement();
        SpellPlacementManager.Instance?.CancelPlacement();

        // Select this card
        int index = transform.GetSiblingIndex();
        deckManager.SelectCard(index);
    }

    public void Setup(Card card, DeckManager manager)
    {
        CardData = card;
        deckManager = manager;

        if (nameText != null) nameText.text = card.cardName;
        if (artworkImage != null) artworkImage.sprite = card.artwork;
        if (descriptionText != null) descriptionText.text = card.description;

        // Set rarity
        if (rarityBackground != null)
        {
            SetRarity(card.rarity);
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                OnCardClicked();
                deckManager.RequestPlayCard(CardData, this);
            });
        }
    }

    // ===== Rarity System =====
    public void SetRarity(Rarity rarity)
{
    string hexColor = "#808080"; // Default: Common (gray)
    switch (rarity)
    {
        case Rarity.Common:
            hexColor = "#808080"; // Gray
            break;
        case Rarity.Rare:
            hexColor = "#0e56dbff"; // Blue
            break;
        case Rarity.Epic:
            hexColor = "#9B59B6"; // Magenta/Purple
            break;
        case Rarity.Legendary:
            hexColor = "#F1C40F"; // Yellow/Gold
            break;
    }
    Color color;
    if (ColorUtility.TryParseHtmlString(hexColor, out color))
    {
        rarityBackground.color = color;
    }
}

    // ===== Shop buy support =====
    private Button buyButton;
    public void EnableBuyButton(Action onBuy)
    {
        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(true);
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuy?.Invoke());
        }

        // Disable root button to prevent in-hand play
        if (button != null)
            button.interactable = false;
    }
}