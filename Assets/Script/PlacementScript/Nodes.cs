using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField] private Color hoverColor = Color.green;
    [SerializeField] private Color occupiedColor = Color.red;

    private Renderer rend;
    private Color startColor;
    private bool isOccupied = false;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        startColor = rend.material.color;
    }

    private void OnMouseEnter()
    {
        if (isOccupied) rend.material.color = occupiedColor;
        else rend.material.color = hoverColor;
    }

    private void OnMouseExit()
    {
        rend.material.color = startColor;
    }

    private void OnMouseDown()
    {
        PlacementManager.Instance.PlaceObject(this);
    }

    public Vector3 GetBuildPosition()
    {
        return transform.position;
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }

    public bool HasObject()
    {
        return isOccupied;
    }
}
