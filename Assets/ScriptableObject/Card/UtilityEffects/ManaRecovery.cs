using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Mana Recovery effect
public class ManaRecoveryMB : UtilityEffectMB
{
    public int manaAmount = 2;

    public override void Execute()
    {
        if (PlayerMana.Instance != null)
        {
            PlayerMana.Instance.currentMana += manaAmount;
            if (PlayerMana.Instance.currentMana > PlayerMana.Instance.maxMana)
                PlayerMana.Instance.currentMana = PlayerMana.Instance.maxMana;

            PlayerMana.Instance.OnManaChanged?.Invoke(PlayerMana.Instance.currentMana, PlayerMana.Instance.maxMana);

            Debug.Log($"{effectName} executed: restored {manaAmount} mana!");
        }
    }
}