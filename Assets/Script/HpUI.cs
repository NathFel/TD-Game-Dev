using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HpUI : MonoBehaviour
{
    public TMP_Text hpText;

    void Update ()
    {
        hpText.text = "" + PlayerStats.Hp.ToString();
    }
}
