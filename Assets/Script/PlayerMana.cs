using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMana : MonoBehaviour
{
    public static PlayerMana Instance;

    [Header("Mana Settings")]
    public int maxMana = 10;
    public int currentMana = 10;

    public Action<int, int> OnManaChanged;

    private void Awake()
    {
        Instance = this;
    }

    public bool CanAfford(int cost) => currentMana >= cost;

    public void Spend(int amount)
    {
        currentMana -= amount;
        currentMana = Mathf.Max(currentMana, 0);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public void ReplenishMana()
    {
        currentMana = maxMana;
        OnManaChanged?.Invoke(currentMana, maxMana);
    }
}