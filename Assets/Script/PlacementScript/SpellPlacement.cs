using System;
using UnityEngine;

public class SpellPlacementManager : MonoBehaviour
{
    public static SpellPlacementManager Instance;

    private Card spellCard;
    private bool isPlacing = false;
    private Action onPlaced;
    private Action onCanceled;
    private Camera mainCamera;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!isPlacing || spellCard == null) return;

        if (Input.GetMouseButtonDown(0)) // Left click to place
        {
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                PlaceSpell(hit.point);
            }
        }

        if (Input.GetMouseButtonDown(1)) // Right click to cancel
        {
            CancelPlacement();
        }
    }

    public void StartPlacingSpell(Card card, Action onPlaced = null, Action onCanceled = null)
    {
        if (card == null || card.spellPrefab == null)
        {
            Debug.LogWarning("Invalid spell card or prefab!");
            return;
        }

        if (isPlacing)
        {
            Debug.LogWarning("Already placing a spell!");
            return;
        }

        spellCard = card;
        isPlacing = true;
        this.onPlaced = onPlaced;
        this.onCanceled = onCanceled;

        Debug.Log("Started placing spell: " + card.cardName);
    }

    private void PlaceSpell(Vector3 position)
    {
        if (spellCard == null) return;

        GameObject spellObj = Instantiate(spellCard.spellPrefab, position, Quaternion.identity);
        Spell spell = spellObj.GetComponent<Spell>();
        if (spell != null)
        {
            spell.power = spellCard.spellPower;
            spell.radius = spellCard.spellRadius;
            spell.Duration = spellCard.spellDuration;
        }

        isPlacing = false;
        spellCard = null;

        onPlaced?.Invoke();
        ClearCallbacks();

        Debug.Log("Spell placed at " + position);
    }

    private void CancelPlacement()
    {
        isPlacing = false;
        spellCard = null;

        onCanceled?.Invoke();
        ClearCallbacks();

        Debug.Log("Spell placement canceled.");
    }

    private void ClearCallbacks()
    {
        onPlaced = null;
        onCanceled = null;
    }

    public bool IsPlacingSpell() => isPlacing;
}
