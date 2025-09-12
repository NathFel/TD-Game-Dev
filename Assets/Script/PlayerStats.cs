using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static int Money;
    public int startMoney = 400;
    public int passiveIncome = 10;

    public static int Hp;
    public int startHp = 20;

    void Start ()
    {
        Money = startMoney;
        StartCoroutine(PassiveIncome()); 
        Hp = startHp;
    }

    IEnumerator PassiveIncome() {
        while (true) 
        {
            yield return new WaitForSeconds(1f);
            Money += passiveIncome;
        }
    }
}