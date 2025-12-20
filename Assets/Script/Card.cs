using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card System/Card")]
public class Card : ScriptableObject
{
    [Header("General Info")]
    public string cardName;
    public Sprite artwork;
    [TextArea] public string description;
    public CardType cardType;
    public CardUI.Rarity rarity = CardUI.Rarity.Common;
    public int manaCost = 1;

    [Header("Tower Data")]
    public GameObject towerPrefab;
    public GameObject bulletPrefab;

    public TowerData towerData;
    public int cost = 0;
    public int upgradeLevel = 0;

    [Header("Spell Data")]
    public GameObject spellPrefab;
    public GameObject spellImpactPrefab;
    
    public int spellPower = 0;
    public float spellDuration = 0f;
    public float spellInterval = 0.5f;
    public float spellRadius = 0f;
    public float spellSpeed = 0f;

    [Header("Freeze Settings")]
    public bool isFreezeSpell = false;
    public float freezeDuration = 0f;
    public float freezeAmount = 0f;
    
    [Header("Utility Card Data")]
    public GameObject utilityEffectPrefab;

    public enum CardType
    {
        Tower,
        Spell,
        Utility
    }
}

[System.Serializable]
public class TowerData
{
    [Header("Tower Stats")]
    public float range = 15f;
    public float fireRate = 1f;
    public float turnSpeed = 10f;
    public int baseDamage = 50;
    
    [Header("Summon")]
    public bool isSummoner = false;
    public GameObject minionPrefab;
    public float minionSpeed = 5f;
    public float minionSpawnYOffset = 1f; 

    [Header("Laser")]
    public bool useLaser = false;
    public float laserLength = 20f;
    public float laserWidth = 1f;

    [Header("Bullet")]
    public bool isCatapult = false;
    public float arcHeight = 5f;
    public int damageOverTime = 10;

    public bool isSeeking = false;
    public int pierce = 0;
    public int bounce = 0;
    public float AoERadius = 0f;
    public GameObject impactPrefab;
    public float AoEDuration = 0.1f;
    public float AoEInterval = 1f;
    public float bulletSpeed = 70f;

    [Header("Burning Settings")]
    [Range(0f, 1f)] public float burnChance = 0.0f;
    public int burnDamage = 1;
}
