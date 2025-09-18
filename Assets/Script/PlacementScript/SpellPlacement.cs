using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpellPlacementManager : MonoBehaviour
{
    public static SpellPlacementManager Instance;

    private Card spellCard;
    private bool isPlacing = false;
    private Action onPlaced;
    private Action onCanceled;
    private Camera mainCamera;

    private GameObject spellPreview;
    public GameObject spellPreviewPrefab;

    [Header("Preview Settings")]
    public LayerMask groundLayer; // assign your ground layer in inspector
    public float previewY = 0.5f; // height of preview above ground
    public float previewLerpSpeed = 20f; // smooth movement speed

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!isPlacing || spellCard == null) return;

        UpdateSpellPreview();

        // Place spell on left-click, ignore clicks over UI
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;

            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                PlaceSpell(hit.point);
            }
        }
    }

    /// <summary>
    /// Starts the spell placement process.
    /// </summary>
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

        CreateSpellPreview();

        Debug.Log("Started placing spell: " + card.cardName);
    }

    private void PlaceSpell(Vector3 position)
    {
        if (spellCard == null) return;

        // Instantiate spell prefab
        GameObject spellObj = Instantiate(spellCard.spellPrefab, position, Quaternion.identity);
        Spell spell = spellObj.GetComponent<Spell>();
        if (spell != null)
        {
            spell.power = spellCard.spellPower;
            spell.radius = spellCard.spellRadius;
            spell.Duration = spellCard.spellDuration;
        }

        DestroyPreview();
        isPlacing = false;
        spellCard = null;

        onPlaced?.Invoke();
        ClearCallbacks();

        Debug.Log("Spell placed at " + position);
    }

    /// <summary>
    /// Cancels the current placement.
    /// </summary>
    public void CancelPlacement()
    {
        DestroyPreview();
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

    // --- Preview Methods ---
    private void CreateSpellPreview()
    {
        if (spellPreviewPrefab != null)
        {
            spellPreview = Instantiate(spellPreviewPrefab);
            float diameter = spellCard.spellRadius * 2f;
            spellPreview.transform.localScale = new Vector3(diameter, 0.1f, diameter);
        }
    }

    private void UpdateSpellPreview()
    {
        if (spellPreview == null) return;

        // Only move preview on ground
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector3 targetPos = new Vector3(hit.point.x, previewY, hit.point.z);
            spellPreview.transform.position = Vector3.Lerp(spellPreview.transform.position, targetPos, Time.deltaTime * previewLerpSpeed);
        }
    }

    private void DestroyPreview()
    {
        if (spellPreview != null)
        {
            Destroy(spellPreview);
            spellPreview = null;
        }
    }

    /// <summary>
    /// Checks if the mouse is currently over any UI element.
    /// </summary>
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
