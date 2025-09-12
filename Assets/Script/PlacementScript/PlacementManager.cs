using System;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    private GameObject objectToPlace;
    private Action onPlaced;
    private Action onCanceled;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Start placing a prefab, with optional callbacks
    public void StartPlacing(GameObject prefab, Action onPlaced = null, Action onCanceled = null)
    {
        objectToPlace = prefab;
        this.onPlaced = onPlaced;
        this.onCanceled = onCanceled;
    }

    private void Update()
    {
        // Cancel with right-click
        if (objectToPlace != null && Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    public void PlaceObject(Node node)
    {
        if (objectToPlace == null || node.HasObject()) return;

        Instantiate(objectToPlace, node.GetBuildPosition(), Quaternion.identity);
        node.SetOccupied(true);

        objectToPlace = null;

        onPlaced?.Invoke();
        ClearCallbacks();
    }

    private void CancelPlacement()
    {
        objectToPlace = null;
        onCanceled?.Invoke();
        ClearCallbacks();
    }

    private void ClearCallbacks()
    {
        onPlaced = null;
        onCanceled = null;
    }
}
