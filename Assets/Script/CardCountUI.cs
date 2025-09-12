using UnityEngine;
using TMPro;

public class CardCountUI : MonoBehaviour
{
    [Header("UI Text References")]
    public TextMeshProUGUI currentDeckText;  // Total cards owned
    public TextMeshProUGUI drawPileText;     // Cards left in draw pile
    public TextMeshProUGUI discardPileText;  // Cards in discard pile

    private DeckManager deckManager;

    private void Awake()
    {
        // Find DeckManager in the scene
        deckManager = FindObjectOfType<DeckManager>();
        if (deckManager == null)
        {
            Debug.LogError("CardCountUI: No DeckManager found in the scene!");
        }
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    /// <summary>
    /// Call this function to update the counts in UI.
    /// </summary>
    public void UpdateUI()
    {
        if (deckManager == null) return;

        if (currentDeckText != null)
            currentDeckText.text = $"Deck: {deckManager.currentDeck.Count}/{deckManager.maxDeckSize}";

        if (drawPileText != null)
            drawPileText.text = $"Draw Pile: {deckManager.drawPile.Count}";

        if (discardPileText != null)
            discardPileText.text = $"Discard: {deckManager.discardPile.Count}";
    }

    /// <summary>
    /// Optional: Update counts every frame (if you want live updates).
    /// </summary>
    private void Update()
    {
        UpdateUI();
    }
}
