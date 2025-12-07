using UnityEngine;
using System;

public class UtilityPlacementManager : MonoBehaviour
{
    public static UtilityPlacementManager Instance;

    private Action onPlaced;
    private Action onCanceled;
    private Card utilityCard;

    private bool isPlacing = false;

    void Awake()
    {
        Instance = this;
    }

    public void StartPlacingUtility(Card card, Action onPlaced, Action onCanceled)
    {
        this.utilityCard = card;
        this.onPlaced = onPlaced;
        this.onCanceled = onCanceled;

        isPlacing = true;
        Debug.Log("Utility card selected. Click anywhere to activate.");
    }

    void Update()
    {
        if (!isPlacing) return;

        // CANCEL if right-click
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
            return;
        }

        // LEFT CLICK activates the utility card
        if (Input.GetMouseButtonDown(0))
        {
            onPlaced?.Invoke();
            FinishPlacement();
        }
    }

    public void CancelPlacement()
    {
        if (!isPlacing) return;

        Debug.Log("Utility card canceled.");
        onCanceled?.Invoke();
        isPlacing = false;
        utilityCard = null;
    }

    private void FinishPlacement()
    {
        isPlacing = false;
        utilityCard = null;

        DeckManager.Instance.isPlacing = false;
    }
}
