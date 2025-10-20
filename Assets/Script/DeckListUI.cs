using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckListUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject listPanel;       // The panel to enable/disable
    public Transform contentArea;      // ScrollView content area
    public CardUI cardUIPrefab;        // Prefab for each card
    public TMP_Text titleText;         // Title ("Current Deck", "Discard Pile", etc.)
    public Button closeButton;         // Close button

    private bool isVisible = false;
    private string currentView = "";

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(HideList);
        HideList();
    }

    public void ShowDeck(List<Card> cards, string title)
    {
        if (listPanel == null || contentArea == null || cardUIPrefab == null)
        {
            Debug.LogError("DeckListUI is missing references!");
            return;
        }

        // Toggle same list if already open
        if (isVisible && currentView == title)
        {
            HideList();
            return;
        }

        currentView = title;
        isVisible = true;
        listPanel.SetActive(true);

        titleText.text = $"{title} ({cards.Count})";

        // Clear old cards
        foreach (Transform child in contentArea)
            Destroy(child.gameObject);

        // Spawn each card UI
        foreach (Card c in cards)
        {
            CardUI cu = Instantiate(cardUIPrefab, contentArea);
            cu.Setup(c, null);
            cu.SetInteractable(false);
        }
    }

    public void HideList()
    {
        if (listPanel != null)
            listPanel.SetActive(false);

        isVisible = false;
        currentView = "";
    }
}
