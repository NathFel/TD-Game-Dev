using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaUI : MonoBehaviour
{
    public Image manaFillImage;      
    public TMP_Text manaText;       

    private void Start()
    {
        PlayerMana.Instance.OnManaChanged += UpdateManaUI;

        // Initialize UI
        UpdateManaUI(PlayerMana.Instance.currentMana, PlayerMana.Instance.maxMana);
    }

    private void UpdateManaUI(int current, int max)
    {
        manaFillImage.fillAmount = (float)current / max;
        manaText.text = $"{current}/{max}";
    }
}
