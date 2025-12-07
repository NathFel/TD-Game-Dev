using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance;

    [Header("Global Settings")]
    public float minionSpawnDelay = 0.5f; // delay between spawns globally

    private bool isSpawning = false;
    private Queue<System.Action> summonQueue = new Queue<System.Action>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Turret calls this to request a summon
    public void RequestSummon(System.Action summonAction)
    {
        summonQueue.Enqueue(summonAction);

        if (!isSpawning)
        {
            StartCoroutine(HandleQueue());
        }
    }

    private IEnumerator HandleQueue()
    {
        isSpawning = true;

        while (summonQueue.Count > 0)
        {
            var action = summonQueue.Dequeue();
            action.Invoke(); // spawn minion

            // wait global delay before next minion
            yield return new WaitForSeconds(minionSpawnDelay);
        }

        isSpawning = false;
    }
}
