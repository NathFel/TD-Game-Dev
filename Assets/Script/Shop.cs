using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public TurretBlueprint standardTurret;
    public TurretBlueprint missileTurret;

    private BuildManager buildManager;

    void Start()
    {
        buildManager = BuildManager.instance;
    }

    public void SelectStandardTurret() 
    {
        buildManager.SelectTurretToBuild(standardTurret);
    }

    public void SelectMissileTurret() 
    {
        buildManager.SelectTurretToBuild(missileTurret);
    }
}
