using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{

    public static int Money;
    public int startMoney = 400;

    public static int Hp;
    public int startHp = 20;

    void Start ()
    {
        Money = startMoney;
        Hp = startHp;
    }

    void Update ()
    {
        if (Hp <= 0)
        {
            if (GameOverUI.Instance == null) return;

            int round = DeckManager.Instance != null ? DeckManager.Instance.currentRound : 1;
            int wave = DeckManager.Instance != null ? DeckManager.Instance.currentWave : 1;

            bool playerWon = round > 10;

            GameOverUI.Instance.ShowGameOver(round, wave, playerWon);
        }
    }

    public void AddMoney(int amount)
    {
        Money += amount;
    }
}