using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardHandDraw3MB : UtilityEffectMB
{
    public int drawAmount = 5;

    public override void Execute()
    {
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DiscardHand();
            DeckManager.Instance.DrawHand(drawAmount); 
            Debug.Log($"Utility Effect: Discarded hand and drew {drawAmount} cards.");
        }
    }
}