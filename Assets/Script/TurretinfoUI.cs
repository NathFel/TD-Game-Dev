using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurretInfoUI : MonoBehaviour
{
    public static TurretInfoUI Instance;

    [Header("UI References")]
    public GameObject panel;
    public TMP_Text nameText;
    public TMP_Text damageText;
    public TMP_Text rangeText;
    public TMP_Text fireRateText;
    public Image turretImage;

    [Header("Targeting Mode")]
    public Button switchModeButton;
    public TMP_Text modeText;

    private Turret currentTurret;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);

        if (switchModeButton != null)
            switchModeButton.onClick.AddListener(OnSwitchModeClicked);
    }

    public void ShowTurretInfo(Turret turret)
    {
        if (turret == null || turret.turretCard == null || turret.turretCard.towerData == null)
        {
            Hide();
            return;
        }

        currentTurret = turret;
        panel.SetActive(true);
        RefreshUI();
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentTurret = null;
    }

    private void OnSwitchModeClicked()
    {
        if (currentTurret == null) return;

        // Cycle to next targeting mode
        int next = ((int)currentTurret.targetingMode + 1) %
                   System.Enum.GetValues(typeof(Turret.TargetingMode)).Length;
        currentTurret.targetingMode = (Turret.TargetingMode)next;

        RefreshUI();

        // ✅ Optionally force re-target immediately
        currentTurret.SendMessage("UpdateTarget", SendMessageOptions.DontRequireReceiver);
    }

    private void RefreshUI()
    {
        if (currentTurret == null || currentTurret.turretCard == null) return;

        var data = currentTurret.turretCard.towerData;

        nameText.text = currentTurret.turretCard.cardName;
        damageText.text = $"Damage: {data.baseDamage}";
        rangeText.text = $"Range: {data.range}";
        fireRateText.text = $"Fire Rate: {data.fireRate:F2}/s";

        if (currentTurret.turretCard.artwork != null)
            turretImage.sprite = currentTurret.turretCard.artwork;

        modeText.text = $"{currentTurret.targetingMode}";
    }
}
