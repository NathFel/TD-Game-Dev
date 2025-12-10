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
    public Transform baseTransform; 

    [Header("Preview Settings")]
    public LayerMask groundLayer;
    public float previewY = 1f;
    public float previewLerpSpeed = 100f;

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

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            

            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                PlaceSpell(hit.point);
            }
        }
    }

    public void StartPlacingSpell(Card card, Action onPlaced = null, Action onCanceled = null)
    {
        if (card == null || card.spellPrefab == null)
        {
            Debug.LogWarning("Invalid spell card or prefab!");
            return;
        }

        spellCard = card;
        isPlacing = true;
        this.onPlaced = onPlaced;
        this.onCanceled = onCanceled;

        CreateSpellPreview();
        Debug.Log("Started placing spell: " + card.cardName);
    }

    private void PlaceSpell(Vector3 targetPosition)
    {
        if (spellCard == null) return;

        // Instantiate the spell prefab at base
        GameObject spellObj = Instantiate(spellCard.spellPrefab, baseTransform.position, Quaternion.identity);
        Spell spell = spellObj.GetComponent<Spell>();
        if (spell != null)
        {
            // Assign all card values to the spell instance
            spell.power = spellCard.spellPower;
            spell.radius = spellCard.spellRadius;
            spell.Duration = spellCard.spellDuration;
            spell.interval = spellCard.spellInterval;
            spell.isFreezeSpell = spellCard.isFreezeSpell;
            spell.freezeDuration = spellCard.freezeDuration;
            spell.freezeSlowMultiplier = spellCard.freezeAmount;

            // <-- Assign impact prefab from the card if provided; otherwise keep prefab's default
            // Prefer card's impact prefab only if it looks like a real FX (has ParticleSystem or CFXR_Effect)
            if (spellCard.spellImpactPrefab != null)
            {
                bool hasPS = spellCard.spellImpactPrefab.GetComponentInChildren<ParticleSystem>(true) != null;
                bool hasCFXR = spellCard.spellImpactPrefab.GetComponentInChildren<CartoonFX.CFXR_Effect>(true) != null;
                if (hasPS || hasCFXR)
                {
                    spell.impactPrefab = spellCard.spellImpactPrefab;
                }
                else
                {
                    Debug.LogWarning($"Card '{spellCard.cardName}' impact prefab '{spellCard.spellImpactPrefab.name}' has no ParticleSystem/CFXR_Effect. Using Spell prefab's impact instead.");
                }
            }

            Debug.Log($"Placing spell '{spellCard.cardName}' with impact prefab: '{(spell.impactPrefab != null ? spell.impactPrefab.name : "<none>")}'");

            // Optional: assign Renderer for hiding projectile
            spell.spellRenderer = spellObj.GetComponentInChildren<Renderer>();

            // Launch spell towards target
            spell.Launch(baseTransform.position, targetPosition);
        }

        DestroyPreview();
        isPlacing = false;
        spellCard = null;
        onPlaced?.Invoke();
        ClearCallbacks();
    }

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

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
