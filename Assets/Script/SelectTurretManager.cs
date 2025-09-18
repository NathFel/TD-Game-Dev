using UnityEngine;

public class TurretSelectionManager : MonoBehaviour
{
    public LayerMask turretLayer;
    private Turret selectedTurret;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, turretLayer))
            {
                Turret turret = hit.collider.GetComponent<Turret>();
                if (turret != null)
                {
                    SelectTurret(turret);
                }
            }
            else
            {
                DeselectCurrentTurret();
            }
        }
    }

    private void SelectTurret(Turret turret)
    {
        if (selectedTurret != null)
            selectedTurret.Deselect();

        selectedTurret = turret;
        selectedTurret.Select();
    }

    private void DeselectCurrentTurret()
    {
        if (selectedTurret != null)
        {
            selectedTurret.Deselect();
            selectedTurret = null;
        }
    }
}
