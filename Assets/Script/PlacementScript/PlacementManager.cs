using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    private GameObject objectToPlacePrefab;
    private GameObject ghostInstance;
    private GameObject rangeSphereInstance;

    private Action onPlaced;
    private Action onCanceled;

    [Header("Ghost Settings")]
    public Material ghostMaterial;       // Transparent ghost material
    public GameObject rangeSpherePrefab; // Transparent sphere prefab

    // Store original materials for restoring
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartPlacing(GameObject prefab, Action onPlaced = null, Action onCanceled = null)
    {
        objectToPlacePrefab = prefab;
        this.onPlaced = onPlaced;
        this.onCanceled = onCanceled;

        // Don’t spawn ghost immediately → wait until hovering a node
        ghostInstance = null;
        rangeSphereInstance = null;
    }

    private void Update()
    {
        if (objectToPlacePrefab == null) return;

        Node node = GetNodeUnderMouse();
        if (node != null)
        {
            // If ghost doesn't exist yet, spawn it now
            if (ghostInstance == null)
                SpawnGhost(objectToPlacePrefab);

            // Snap ghost to node
            Vector3 buildPos = node.GetBuildPosition();
            ghostInstance.transform.position = buildPos;

            if (rangeSphereInstance != null)
                rangeSphereInstance.transform.position = buildPos + Vector3.up * 0.5f;

            // Left click -> place
            if (Input.GetMouseButtonDown(0) && !node.HasObject())
            {
                PlaceObject(node);
            }
        }
        else
        {
            // If not hovering node, hide ghost (instead of following ground)
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }
            if (rangeSphereInstance != null)
            {
                Destroy(rangeSphereInstance);
                rangeSphereInstance = null;
            }
        }
    }

    private void SpawnGhost(GameObject prefab)
    {
        ghostInstance = Instantiate(prefab);
        ghostInstance.name = prefab.name + "_Ghost";
        ApplyGhostMaterial(ghostInstance);
        DisableGhostColliders(ghostInstance);

        // Create range sphere
        Turret turret = prefab.GetComponent<Turret>();
        if (turret != null && rangeSpherePrefab != null)
        {
            float diameter = turret.turretCard.towerData.range * 2f;
            rangeSphereInstance = Instantiate(rangeSpherePrefab, ghostInstance.transform.position, Quaternion.identity);
            rangeSphereInstance.transform.localScale = new Vector3(diameter, 0.1f, diameter);
        }
    }

    public void PlaceObject(Node node)
    {
        if (objectToPlacePrefab == null || node.HasObject()) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        return;

        GameObject placed = Instantiate(objectToPlacePrefab, node.GetBuildPosition(), Quaternion.identity);
        node.SetOccupied(true, placed);

        CleanupGhost();
        onPlaced?.Invoke();
        ClearCallbacks();
    }

    public void CancelPlacement()
    {
        CleanupGhost();
        objectToPlacePrefab = null;
        onCanceled?.Invoke();
        ClearCallbacks();
    }

    private void CleanupGhost()
    {
        if (ghostInstance != null)
        {
            RestoreOriginalMaterials();
            Destroy(ghostInstance);
        }
        if (rangeSphereInstance != null)
            Destroy(rangeSphereInstance);

        ghostInstance = null;
        rangeSphereInstance = null;
        objectToPlacePrefab = null;
    }

    private void ClearCallbacks()
    {
        onPlaced = null;
        onCanceled = null;
    }

    private void ApplyGhostMaterial(GameObject obj)
    {
        originalMaterials.Clear();
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            originalMaterials[r] = r.materials; // Save originals
            Material[] ghostMats = new Material[r.materials.Length];
            for (int i = 0; i < ghostMats.Length; i++)
            {
                ghostMats[i] = ghostMaterial;
            }
            r.materials = ghostMats;
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var pair in originalMaterials)
        {
            if (pair.Key != null)
                pair.Key.materials = pair.Value;
        }
        originalMaterials.Clear();
    }

    private void DisableGhostColliders(GameObject obj)
    {
        foreach (Collider c in obj.GetComponentsInChildren<Collider>())
        {
            Destroy(c); // Remove colliders so ghost doesn’t block clicks
        }
    }

    private Node GetNodeUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponent<Node>();
        }
        return null;
    }
}
