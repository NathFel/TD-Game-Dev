using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField] private float yOffset = 0.5f; // for actual placement

    public GameObject previewEmptyPrefab;   
    public GameObject previewOccupiedPrefab;
    public float emptyYOffset = 0.5f;
    public float occupiedYOffset = 1f;
    
    [Header("Node Settings")]
    public bool blockTowerPlacement = false; // Toggle to prevent tower placement on this node

    private GameObject currentPreview;
    private Renderer rend;

    private bool isOccupied = false;
    private GameObject currentObject; // store placed tower
    

    private void Start()
    {
        rend = GetComponent<Renderer>();
    }

    private void OnMouseEnter()
    {
        if (currentPreview != null) Destroy(currentPreview);

        Vector3 spawnPos = transform.position;

        if (isOccupied && previewOccupiedPrefab != null)
        {
            spawnPos.y += occupiedYOffset;
            currentPreview = Instantiate(previewOccupiedPrefab, spawnPos, Quaternion.identity);
        }
        else if (!isOccupied && previewEmptyPrefab != null)
        {
            spawnPos.y += emptyYOffset;
            currentPreview = Instantiate(previewEmptyPrefab, spawnPos, Quaternion.identity);
        }
    }

    private void OnMouseExit()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }

    private void OnMouseDown()
    {
        PlacementManager.Instance.PlaceObject(this);
    }

    public Vector3 GetBuildPosition()
    {
        // Build position uses yOffset for actual placement
        Vector3 pos = transform.position;
        pos.y += yOffset;
        return pos;
    }

    public void SetOccupied(bool occupied, GameObject placedObj = null)
    {
        isOccupied = occupied;
        currentObject = placedObj;
    }

    public bool HasObject()
    {
        return isOccupied;
    }

    public bool CanPlaceTower()
    {
        return !blockTowerPlacement && !isOccupied;
    }

    public void ClearNode()
    {
        if (currentObject != null)
        {
            Destroy(currentObject);
            currentObject = null;
        }

        isOccupied = false;
    }
}
