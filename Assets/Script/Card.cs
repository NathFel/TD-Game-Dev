using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card System/Card")]
public class Card : ScriptableObject
{
    [Header("General Info")]
    public string cardName;
    public Sprite artwork;
    [TextArea] public string description;
    public CardType cardType;

    [Header("Tower Data")]
    public GameObject towerPrefab;
    public int cost = 0; // how much it costs in shop
    public int upgradeLevel = 0; // if it’s an upgrade card

    [Header("Spell Data")]
    public GameObject spellPrefab;
    public int spellPower = 0; 
    public float spellDuration = 0f;
    public float spellRadius = 0f;
    public float spellSpeed = 0f;

    public enum CardType
    {
        Tower,
        Spell,
        Upgrade
    }
}
