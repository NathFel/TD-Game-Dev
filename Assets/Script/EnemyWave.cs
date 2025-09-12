using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    public static EnemyWaveManager Instance;

    [Header("Enemy Settings")]
    public Transform spawnPoint;
    public List<EnemyTypeDataSO> enemyTypes;

    [Header("Wave Base Stats")]
    public float baseHealth = 100f;
    public float baseSpeed = 1f;
    public float baseMoney = 10f;

    [Header("Wave Scaling")]
    public float roundMultiplier = 1.5f;   // multiplicative per round
    public float waveIncrementHealth = 10f; // additive per wave
    public float waveIncrementSpeed = 0.1f; 
    public float waveIncrementMoney = 1f;

    [Header("Wave Settings")]
    public int startingEnemyCount = 5;
    public float spawnInterval = 1f;

    private int enemiesAlive = 0;
    private Action onWaveComplete;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one EnemyWaveManager in scene!");
            return;
        }
        Instance = this;
    }

    public void StartWave(int roundNumber, int waveNumber, Action onComplete)
    {
        onWaveComplete = onComplete;
        int enemyCount = Mathf.RoundToInt(startingEnemyCount * (1 + 0.5f * (waveNumber - 1)));
        StartCoroutine(SpawnWave(enemyCount, roundNumber, waveNumber));
    }

    private IEnumerator SpawnWave(int count, int roundNumber, int waveNumber)
    {
        enemiesAlive = count;

        for (int i = 0; i < count; i++)
        {
            EnemyTypeDataSO enemyData = GetRandomEnemy(roundNumber);
            if (enemyData != null)
                SpawnEnemy(enemyData, roundNumber, waveNumber);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private EnemyTypeDataSO GetRandomEnemy(int currentRound)
    {
        List<EnemyTypeDataSO> available = enemyTypes.FindAll(e => currentRound >= e.minRoundToSpawn);
        if (available.Count == 0) return null;

        // Weighted random selection
        float totalWeight = 0f;
        foreach (var e in available) totalWeight += e.spawnWeight;

        float rnd = UnityEngine.Random.Range(0f, totalWeight);
        float sum = 0f;
        foreach (var e in available)
        {
            sum += e.spawnWeight;
            if (rnd <= sum) return e;
        }

        return available[available.Count - 1];
    }

    private void SpawnEnemy(EnemyTypeDataSO data, int roundNumber, int waveNumber)
    {
        GameObject obj = Instantiate(data.prefab, spawnPoint.position, Quaternion.identity);
        Enemy enemy = obj.GetComponent<Enemy>();
        if (enemy == null) return;

        // Centralized scaling
        float scaledHealth = baseHealth * Mathf.Pow(roundMultiplier, roundNumber - 1) + waveIncrementHealth * (waveNumber - 1);
        float scaledSpeed = baseSpeed * Mathf.Pow(roundMultiplier, roundNumber - 1) + waveIncrementSpeed * (waveNumber - 1);
        float scaledMoney = baseMoney * Mathf.Pow(roundMultiplier, roundNumber - 1) + waveIncrementMoney * (waveNumber - 1);

        // Apply enemy type multiplier
        enemy.health = Mathf.RoundToInt(scaledHealth * data.healthMultiplier);
        enemy.speed = scaledSpeed * data.speedMultiplier;
        enemy.moneyDrop = Mathf.RoundToInt(scaledMoney * data.moneyMultiplier);

        obj.SetActive(true);
    }

    public void EnemyDied()
    {
        enemiesAlive--;
        if (enemiesAlive <= 0 && onWaveComplete != null)
            onWaveComplete.Invoke();
    }
}
