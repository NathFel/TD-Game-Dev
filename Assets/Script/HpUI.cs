using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HpUI : MonoBehaviour
{
    public TMP_Text hpText;
    public TMP_Text moneyText;

    void Update ()
    {
        if (hpText != null)
            hpText.text = PlayerStats.Hp.ToString();

        if (moneyText != null)
            moneyText.text = $"{PlayerStats.Money}$";
    }
}
