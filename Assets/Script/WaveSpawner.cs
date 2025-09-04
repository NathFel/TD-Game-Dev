using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

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
        if(countdown <= 0) {
            StartCoroutine(SpawnWave());
            countdown = timeBetweenWave;
        }

        countdown -= Time.deltaTime;
        
        StartCoroutine(UpdateCountdownText());
    }

    IEnumerator UpdateCountdownText()
{
    while (true) {
        WaveCount.text = Mathf.Round(countdown).ToString();
        yield return new WaitForSeconds(2f);
    }
}
    IEnumerator SpawnWave() {
        waveIndex++;
        
        for (int i = 0; i < waveIndex; i++) {
            SpawnEnemy();
            yield return new WaitForSeconds(0.5f);
        }
    }

    void SpawnEnemy() {
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
