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
    public GameObject bulletPrefab;
    public TowerData towerData; 
    public int cost = 0; 
    public int upgradeLevel = 0;            

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

[System.Serializable]
public class TowerData
{
    [Header("Tower Stats")]
    public float range = 15f;
    public float fireRate = 1f;
    public float turnSpeed = 10f;
    public int baseDamage = 50;  
    [Header("Laser")]
    public bool useLaser = false;
    public float laserLength = 20f;
    public float laserWidth = 1f;
    [Header("Bullet")]
    public bool isSeeking = false;
    public int pierce = 0;
    public int bounce = 0;
    public float AoERadius = 0f;
    public float bulletSpeed = 70f;
    [Header("Burning Settings")]
    [Range(0f, 1f)] public float burnChance = 0.0f;  
    public int burnDamage = 1; 
}
