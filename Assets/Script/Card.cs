using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card System/Card")]
public class Card : ScriptableObject
{
    [Header("General Info")]
    public string cardName;
    public Sprite artwork;
    [TextArea] public string descriptionTemplate;
    [HideInInspector] public string description;
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

    #if UNITY_EDITOR
    private void OnValidate()
    {
        description = GenerateDescription();
    }
    #endif

    private string GenerateDescription()
    {
        string d = descriptionTemplate;

        // =========================
        // GENERAL
        // =========================
        d = d.Replace("{cardName}", cardName);
        d = d.Replace("{rarity}", rarity.ToString());
        d = d.Replace("{manaCost}", manaCost.ToString());
        d = d.Replace("{cost}", cost.ToString());
        d = d.Replace("{upgradeLevel}", upgradeLevel.ToString());

        // =========================
        // SPELL
        // =========================
        d = d.Replace("{spellPower}", spellPower.ToString());
        d = d.Replace("{spellDuration}", spellDuration.ToString("0.##"));
        d = d.Replace("{spellInterval}", spellInterval.ToString("0.##"));
        d = d.Replace("{spellRadius}", spellRadius.ToString("0.##"));
        d = d.Replace("{spellSpeed}", spellSpeed.ToString("0.##"));

        // =========================
        // FREEZE
        // =========================
        d = d.Replace("{isFreezeSpell}", isFreezeSpell ? "Yes" : "No");
        d = d.Replace("{freezeDuration}", freezeDuration.ToString("0.##"));
        d = d.Replace("{freezeAmount}", (freezeAmount * 100f).ToString("0") + "%");

        // =========================
        // TOWER
        // =========================
        if (towerData != null)
        {
            d = d.Replace("{range}", towerData.range.ToString("0.##"));
            
            string intervalText = towerData.fireRate <= 0
                ? "instantly"
                : $"every {towerData.fireRate:0.##} second{(towerData.fireRate > 1 ? "s" : "")}";

            d = d.Replace("{damageInterval}", intervalText);
            
            d = d.Replace("{turnSpeed}", towerData.turnSpeed.ToString("0.##"));
            d = d.Replace("{baseDamage}", towerData.baseDamage.ToString());

            d = d.Replace("{isSummoner}", towerData.isSummoner ? "Yes" : "No");
            d = d.Replace("{minionSpeed}", towerData.minionSpeed.ToString("0.##"));
            d = d.Replace("{minionSpawnYOffset}", towerData.minionSpawnYOffset.ToString("0.##"));

            d = d.Replace("{useLaser}", towerData.useLaser ? "Yes" : "No");
            d = d.Replace("{laserLength}", towerData.laserLength.ToString("0.##"));
            d = d.Replace("{laserWidth}", towerData.laserWidth.ToString("0.##"));

            d = d.Replace("{isCatapult}", towerData.isCatapult ? "Yes" : "No");
            d = d.Replace("{arcHeight}", towerData.arcHeight.ToString("0.##"));
            d = d.Replace("{damageOverTime}", towerData.damageOverTime.ToString());
            d = d.Replace("{isSeeking}", towerData.isSeeking ? "Yes" : "No");
            d = d.Replace("{pierce}", towerData.pierce.ToString());
            d = d.Replace("{bounce}", towerData.bounce.ToString());
            d = d.Replace("{AoERadius}", towerData.AoERadius.ToString("0.##"));
            d = d.Replace("{AoEDuration}", towerData.AoEDuration.ToString("0.##"));
            d = d.Replace("{AoEInterval}", towerData.AoEInterval.ToString("0.##"));
            d = d.Replace("{bulletSpeed}", towerData.bulletSpeed.ToString("0.##"));

            d = d.Replace("{burnChance}", (towerData.burnChance * 100f).ToString("0") + "%");
            d = d.Replace("{burnDamage}", towerData.burnDamage.ToString());
        }

        return d;
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
