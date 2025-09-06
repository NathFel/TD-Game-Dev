using System.Collections;
using UnityEngine;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    public Transform enemyPrefab;

    public float timeBetweenWave = 5f;
    private float countdown = 2f;
    public TMP_Text WaveCount;

    public Transform spawnPoint;

    private int waveIndex = 0;

    void Update()
    {
        if (countdown <= 0)
        {
            StartCoroutine(SpawnWave());
            countdown = timeBetweenWave;
        }

        countdown -= Time.deltaTime;
        countdown = Mathf.Clamp(countdown, 0f, Mathf.Infinity);

        // Format countdown
        int minutes = Mathf.FloorToInt(countdown / 60f);
        int seconds = Mathf.FloorToInt(countdown % 60f);
        int centiseconds = Mathf.FloorToInt((countdown * 100f) % 100f);

        if (minutes > 0)
        {
            WaveCount.text = string.Format("{0}:{1:00}.{2:00}", minutes, seconds, centiseconds);
        }
        else
        {
            WaveCount.text = string.Format("{0}.{1:00}", seconds, centiseconds);
        }
    }

    IEnumerator SpawnWave()
    {
        waveIndex++;

        for (int i = 0; i < waveIndex; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.5f);
        }
    }

    void SpawnEnemy()
    {
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
