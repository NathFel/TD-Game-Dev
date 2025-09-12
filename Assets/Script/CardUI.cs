using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text descriptionText;

    private Button button; // root button
    private DeckManager deckManager;

    // Expose card to DeckManager
    public Card CardData { get; private set; }

    private void Awake()
    {
        // Get the Button component on the prefab root
        button = GetComponent<Button>();
    }

    public void Setup(Card card, DeckManager manager)
    {
        CardData = card;
        deckManager = manager;

        if (nameText != null) nameText.text = card.cardName;
        if (artworkImage != null) artworkImage.sprite = card.artwork;
        if (descriptionText != null) descriptionText.text = card.description;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                deckManager.RequestPlayCard(CardData, this);
            });
        }
    }

    public void SetInteractable(bool value)
    {
        if (button != null) button.interactable = value;
    }

    private Button buyButton; 
    public void EnableBuyButton(Action onBuy)
    {
        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(true);
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuy?.Invoke());
        }

        // Disable root button to prevent playing in-hand
        if (button != null)
            button.interactable = false;
    }
}
