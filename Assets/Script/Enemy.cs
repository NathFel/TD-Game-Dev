using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 10f;
    public int health = 100;
    
    public int moneyDrop = 50;

    private Transform target;
    private int wavepointIndex = 0;

    public GameObject deathEffect;
    
    void Start()
    { 
        target = Waypoints.points[0];
    }

    public void TakeDamage (int amount)
    {
        health -= amount;

        if (health <= 0) {
            Die();
        }
    }

    void Die()
    {
        GameObject effect = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(effect, 5f);
  
        // Notify wave manager
        EnemyWaveManager.Instance.EnemyDied();

        PlayerStats.Money += moneyDrop;
        Destroy(gameObject);
    }

    void Update()
    {
        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, target.position) <= 0.4f) {
            GetNextWaypoint();
        }
    }

    void GetNextWaypoint()
    {
        if (wavepointIndex >= Waypoints.points.Length - 1) { 
            EndPath();
            return; 
        }

        wavepointIndex++;
        target = Waypoints.points[wavepointIndex];

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
    }

    void EndPath ()
    {
        PlayerStats.Hp--;
        // Notify wave manager
        EnemyWaveManager.Instance.EnemyDied();
        Destroy(gameObject);
    }
}
