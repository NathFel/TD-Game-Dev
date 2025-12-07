using UnityEngine;

[CreateAssetMenu(fileName = "New Utility Effect", menuName = "Card System/Utility Effect")]
public class UtilityEffect : ScriptableObject
{
    public string effectName;
    [TextArea] public string description;

    public virtual void Execute()
    {
        Debug.Log($"{effectName} executed.");
    }
}

[CreateAssetMenu(fileName = "DiscardHandDraw3", menuName = "Card System/UtilityEffects/Discard Hand Draw 3")]
public class DiscardHandDraw3Effect : UtilityEffect
{
    public int drawAmount = 3;

    public override void Execute()
    {
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DiscardHand();
            DeckManager.Instance.DrawHand(drawAmount); 
            Debug.Log("Utility Effect: Discarded hand and drew " + drawAmount + " cards.");
        }
    }
}

[CreateAssetMenu(fileName = "ManaRecovery", menuName = "Card System/UtilityEffects/ManaRecovery")]
public class ManaRecovery : UtilityEffect
{
    [Header("Mana Recovery Settings")]
    public int manaAmount = 5;

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

[CreateAssetMenu(fileName = "Draw", menuName = "Card System/UtilityEffects/Draw")]
public class DrawCardsCard : UtilityEffect
{
    public int drawAmount = 2;

    public override void Execute()
    {
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DrawHand(drawAmount); 
        }
    }
}