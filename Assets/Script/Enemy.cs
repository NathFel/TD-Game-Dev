using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyHealthUI))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyTypeDataSO enemyType;

    [Header("Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int baseHealth = 100;
    [SerializeField] private int moneyDrop = 10;

    [HideInInspector] public int wavepointIndex = 0;
    [HideInInspector] public float segmentProgress = 0f;

    [Header("Effects")]
    public GameObject deathEffect;
    public GameObject burnEffectPrefab;

    // --- Burn system variables ---
    private bool isBurning = false;
    private int currentBurnDamage = 0;
    private float burnInterval = 1f;   // Enemy-defined
    private float burnDuration = 5f;   // Enemy-defined
    private float burnTimeElapsed = 0f;
    private GameObject activeBurnEffect;
    private Coroutine burnCoroutine;

    // --- Freeze ---
    private bool isFrozen = false;
    private float originalSpeed;

    // --- Enemy Settings ---
    [HideInInspector]public int currentHealth;
    private int maxHealth;
    private Transform startPoint;
    private Transform endPoint;
    private EnemyHealthUI healthUI;
    private int damageToPlayer;

    public int Health => currentHealth;
    public int MaxHealth => maxHealth;
    public float Speed => speed;
    public int MoneyDrop => moneyDrop;

    void Awake()
    {
        healthUI = GetComponent<EnemyHealthUI>();
        maxHealth = enemyType != null ? Mathf.RoundToInt(baseHealth * enemyType.healthMultiplier) : baseHealth;
        currentHealth = maxHealth;
        healthUI?.SetMaxHealth(maxHealth);
        damageToPlayer = enemyType.enemyDamage;
    }

    void OnEnable()
    {
        EnemyWaveManager.Instance.RegisterEnemy(this);

        if (Waypoints.points.Length > 0)
        {
            wavepointIndex = 0;
            startPoint = Waypoints.points[0];
            endPoint = Waypoints.points[1];
        }
    }

    void Update()
    {
        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        if (wavepointIndex >= Waypoints.points.Length - 1) return;

        float segmentLength = Vector3.Distance(startPoint.position, endPoint.position);
        segmentProgress += (speed * Time.deltaTime) / segmentLength;

        if (segmentProgress >= 1f)
        {
            segmentProgress = 0f;
            wavepointIndex++;

            if (wavepointIndex >= Waypoints.points.Length - 1)
            {
                EndPath();
                return;
            }

            startPoint = Waypoints.points[wavepointIndex];
            endPoint = Waypoints.points[wavepointIndex + 1];
        }

        transform.position = Vector3.Lerp(startPoint.position, endPoint.position, segmentProgress);
        Vector3 dir = (endPoint.position - startPoint.position).normalized;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        healthUI?.UpdateHealth(currentHealth);

        if (currentHealth <= 0) Die();
    }

    public void ApplyBurn(int damage)
    {
        if (isBurning)
        {
            if (damage > currentBurnDamage)
            {
                currentBurnDamage = damage;
                burnTimeElapsed = 0f;
            }
            else
            {
                burnTimeElapsed = 0f;
            }
            return;
        }
    StartBurn(damage);
    }

    private void StartBurn(int damage)
    {
        currentBurnDamage = damage;
        burnTimeElapsed = 0f;

        if (burnEffectPrefab != null && activeBurnEffect == null)
            activeBurnEffect = Instantiate(burnEffectPrefab, transform.position, Quaternion.identity, transform);

        isBurning = true;
        burnCoroutine = StartCoroutine(BurnRoutine());
    }

    private IEnumerator BurnRoutine()
    {
        while (isBurning)
        {
            TakeDamage(currentBurnDamage);

            if (activeBurnEffect != null)
                activeBurnEffect.transform.position = transform.position;

            burnTimeElapsed += burnInterval;

            if (burnTimeElapsed >= burnDuration)
            {
                StopBurn();
                yield break;
            }

            yield return new WaitForSeconds(burnInterval);
        }
    }

    public void StopBurn()
    {
        if (burnCoroutine != null)
            StopCoroutine(burnCoroutine);

        burnCoroutine = null;
        isBurning = false;
        currentBurnDamage = 0;

        if (activeBurnEffect != null)
        {
            Destroy(activeBurnEffect);
            activeBurnEffect = null;
        }
    }

    public void ApplyFreeze(float duration, float slowMultiplier)
    {
        if (!isFrozen)
        {
            isFrozen = true;
            originalSpeed = speed;
            speed *= slowMultiplier; // slow enemy
            StartCoroutine(FreezeTimer(duration));
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FreezeTimer(duration)); // refresh timer if hit again
        }
    }

    private IEnumerator FreezeTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        speed = originalSpeed; // restore speed
        isFrozen = false;
    }

    void Die()
    {
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, 5f);
        }

        StopBurn();

        PlayerStats.Money += moneyDrop;

        EnemyWaveManager.Instance.EnemyDied(this);
        EnemyWaveManager.Instance.UnregisterEnemy(this);

        Destroy(gameObject);
    }


    void EndPath()
    {
        PlayerStats.Hp -= damageToPlayer;
        EnemyWaveManager.Instance.UnregisterEnemy(this);
        EnemyWaveManager.Instance.EnemyDied(this);
        Destroy(gameObject);
    }

    public float GetPathProgress()
    {
        return wavepointIndex + segmentProgress;
    }

    public void SetStats(float newSpeed, int newBaseHealth, int newMoney, EnemyTypeDataSO type)
    {
        speed = newSpeed;
        baseHealth = newBaseHealth;
        moneyDrop = newMoney;
        enemyType = type;

        maxHealth = Mathf.RoundToInt(baseHealth * (enemyType != null ? enemyType.healthMultiplier : 1f));
        currentHealth = maxHealth;

        healthUI?.SetMaxHealth(maxHealth);
    }
}
