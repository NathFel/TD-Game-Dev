using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTypeData", menuName = "Enemy Type", order = 0)]
public class EnemyTypeDataSO : ScriptableObject
{
    public string enemyName;
    public GameObject prefab;
    public float healthMultiplier = 1f;  // 1x for base, 1.5x for elite
    public float speedMultiplier = 1f;
    public float moneyMultiplier = 1f;
    public int minRoundToSpawn = 1;      // Enemy only appears after this round
    public float spawnWeight = 1f;       // Optional for weighted random spawn
}
