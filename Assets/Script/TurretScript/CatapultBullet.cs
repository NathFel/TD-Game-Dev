using UnityEngine;
using System.Collections;

public class CatapultBullet : MonoBehaviour
{
    private TowerData data;

    private Vector3 targetPos;
    private bool landed = false;

    public event System.Action<Enemy> onHitEnemy;

    private float nextTick = 0f;
    private float aoeTime = 0f;
    private GameObject impactObj;

    public void Setup(TowerData td)
    {
        data = td;
    }

    public void Launch(Vector3 pos)
    {
        targetPos = pos;   // fixed snapshot of target location now
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        Vector3 start = transform.position;
        float dist = Vector3.Distance(start, targetPos);
        float travelTime = dist / data.bulletSpeed;

        float elapsed = 0f;

        float arc = data.arcHeight > 0 ? data.arcHeight : 20f;

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / travelTime;

            float height = Mathf.Sin(t * Mathf.PI) * arc;

            transform.position =
                Vector3.Lerp(start, targetPos, t) +
                Vector3.up * height;

            yield return null;
        }

        LandImpact();
    }

    void LandImpact()
    {
        if (landed) return;
        landed = true;

        Collider[] hits = Physics.OverlapSphere(targetPos, data.AoERadius);
        foreach (var h in hits)
        {
            Enemy e = h.GetComponent<Enemy>();
            if (e != null)
            {
                e.TakeDamage(data.baseDamage); 
                onHitEnemy?.Invoke(e);
            }
        }

        // SPAWN IMPACT VISUAL
        if (data.impactPrefab != null)
        {
            Vector3 impactPos = new Vector3(targetPos.x, 0.6f, targetPos.z);
            impactObj = Instantiate(data.impactPrefab, impactPos, Quaternion.identity);
            impactObj.transform.localScale = new Vector3(
                data.AoERadius * 2f,
                0.3f,
                data.AoERadius * 2f
            );
        }

        // START AoE DAMAGE OVER TIME AFTER FIRST INTERVAL
        nextTick = Time.time + data.AoEInterval; 
        aoeTime = 0f;
        StartCoroutine(ApplyAoE());
    }

    IEnumerator ApplyAoE()
    {
        while (aoeTime < data.AoEDuration)
        {
            if (Time.time >= nextTick)
            {
                Collider[] hits = Physics.OverlapSphere(targetPos, data.AoERadius);
                foreach (var h in hits)
                {
                    Enemy e = h.GetComponent<Enemy>();
                    if (e != null)
                    {
                        e.TakeDamage(data.damageOverTime); // same effect damage
                        onHitEnemy?.Invoke(e);
                    }
                }

                nextTick = Time.time + data.AoEInterval;
            }

            aoeTime += Time.deltaTime;
            yield return null;
        }

        if (impactObj != null) Destroy(impactObj);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPos, data.AoERadius);
    }
}
