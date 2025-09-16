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
    public int baseHealth = 100;
    public float baseSpeed = 1f;
    public int baseMoney = 10;

    [Header("Wave Scaling")]
    public float roundMultiplier = 1.5f;
    public float waveIncrementHealth = 10f;
    public float waveIncrementSpeed = 0.1f;
    public float waveIncrementMoney = 1f;

    [Header("Wave Settings")]
    public int startingEnemyCount = 5;
    public float spawnInterval = 1f;

    [HideInInspector] public List<Enemy> activeEnemies = new List<Enemy>();

    private Action onWaveComplete;

    // NEW: Track remaining enemies to spawn
    private int enemiesRemainingToSpawn = 0;

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
        enemiesRemainingToSpawn = Mathf.RoundToInt(startingEnemyCount * (1 + 0.5f * (waveNumber - 1)));
        StartCoroutine(SpawnWave(enemiesRemainingToSpawn, roundNumber, waveNumber));
    }

    private IEnumerator SpawnWave(int count, int roundNumber, int waveNumber)
    {
        for (int i = 0; i < count; i++)
        {
            EnemyTypeDataSO enemyData = GetRandomEnemy(roundNumber);
            if (enemyData != null)
                SpawnEnemy(enemyData, roundNumber, waveNumber);

            enemiesRemainingToSpawn--;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private EnemyTypeDataSO GetRandomEnemy(int currentRound)
    {
        List<EnemyTypeDataSO> available = enemyTypes.FindAll(e => currentRound >= e.minRoundToSpawn);
        if (available.Count == 0) return null;

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

        float scaledSpeed = baseSpeed * Mathf.Pow(roundMultiplier, roundNumber - 1) + waveIncrementSpeed * (waveNumber - 1);
        int scaledHealth = Mathf.RoundToInt(baseHealth * Mathf.Pow(roundMultiplier, roundNumber - 1) + waveIncrementHealth * (waveNumber - 1));
        int scaledMoney = Mathf.RoundToInt(baseMoney * Mathf.Pow(roundMultiplier, roundNumber - 1) + waveIncrementMoney * (waveNumber - 1));

        enemy.SetStats(scaledSpeed * data.speedMultiplier, scaledHealth, Mathf.RoundToInt(scaledMoney * data.moneyMultiplier), data);

        obj.SetActive(true);
    }

    public void EnemyDied(Enemy enemy)
    {
        // Only remove if still active
        if (!activeEnemies.Contains(enemy)) return;

        activeEnemies.Remove(enemy);
        CheckWaveComplete();
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);

        CheckWaveComplete();
    }

    private void CheckWaveComplete()
    {
        // Wave is complete only if:
        // 1. No active enemies, AND
        // 2. No more enemies left to spawn
        if (activeEnemies.Count == 0 && enemiesRemainingToSpawn <= 0 && onWaveComplete != null)
        {
            onWaveComplete.Invoke();
            onWaveComplete = null;
        }
    }
}
