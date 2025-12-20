using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardsMB : UtilityEffectMB
{
    public int drawAmount = 2;

    public override void Execute()
    {
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DrawHand(drawAmount); 
            Debug.Log($"Utility Effect: Drew {drawAmount} cards.");
        }
    }
}