using UnityEngine;
using System.Collections.Generic;

public class MinionUnit : MonoBehaviour
{
    public int hp;
    public float moveSpeed;
    public float yOffset = 0f;

    List<Transform> route;
    int currentWaypoint = 0;

    Animator anim;
    bool isDead = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetRoute(List<Transform> r, float offsetY)
    {
        route = new List<Transform>();

        foreach (Transform wp in r)
        {
            Vector3 shifted = wp.position;
            shifted.y += offsetY;

            GameObject temp = new GameObject("wp");
            temp.transform.position = shifted;

            route.Add(temp.transform);
        }

        yOffset = offsetY;
    }

    void Update()
    {
        if (isDead) return;                   // 👈 added
        if (route == null || route.Count == 0) return;

        if (EnemyWaveManager.Instance.activeEnemies.Count == 0)
        {
            anim.SetTrigger("return");
            Destroy(gameObject, 1f);
            return;
        }

        MoveAlongRoute();
    }

    void MoveAlongRoute()
    {
        if (isDead) return;

        Transform target = route[currentWaypoint];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            currentWaypoint++;

            if (currentWaypoint >= route.Count)
            {
                anim.SetTrigger("return");
                Destroy(gameObject, 1f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        Enemy e = other.GetComponent<Enemy>();
        if (!e) return;

        int enemyHPBefore = e.currentHealth;

        // Minion deals damage
        e.TakeDamage(hp);

        int enemyHPAfter = e.currentHealth;

        // leftover damage enemy would've done back
        int leftover = Mathf.Max(enemyHPAfter, 0);

        // reduce minion HP by enemy remaining BEFORE the hit
        hp -= Mathf.Max(enemyHPBefore - enemyHPAfter, enemyHPBefore);

        // clamp
        hp = Mathf.Max(hp, 0);

        // if minion died
        if (hp <= 0)
        {
            Destroy(gameObject);
            return;
        }
    }
}
