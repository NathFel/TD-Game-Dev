using UnityEngine;
using System.Collections;

public class Spell : MonoBehaviour
{
    [HideInInspector] public int power = 10;
    [HideInInspector] public float radius = 2f;
    [HideInInspector] public float Duration = 2f;
    [HideInInspector] public float interval = 0.5f;

    [HideInInspector] public bool isFreezeSpell = false;
    [HideInInspector] public float freezeDuration = 2f;
    [HideInInspector] public float freezeSlowMultiplier = 0.5f;

    [Header("Projectile")]
    public float speed = 50f;
    public float arcHeight = 20f;

    private Vector3 targetPosition;
    private bool hasLanded = false;
    private float nextTickTime = 0f;

    [Header("Visuals")]
    public GameObject impactPrefab; // AoE particle prefab
    public Renderer spellRenderer;  // assign the spell mesh renderer here

    /// <summary>
    /// Launch the spell projectile
    /// </summary>
    public void Launch(Vector3 start, Vector3 target)
    {
        transform.position = start;
        targetPosition = target;
        nextTickTime = 0f;
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        Vector3 startPos = transform.position;
        float journeyLength = Vector3.Distance(startPos, targetPosition);
        float travelTime = journeyLength / speed;
        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / travelTime;

            // Parabolic arc
            float height = Mathf.Sin(t * Mathf.PI) * arcHeight;
            transform.position = Vector3.Lerp(startPos, targetPosition, t) + Vector3.up * height;

            yield return null;
        }

        // Landed
        transform.position = targetPosition;
        hasLanded = true;

        // Hide the spell visually
        if (spellRenderer != null)
            spellRenderer.enabled = false;

        // Spawn impact visual
        if (impactPrefab != null)
        {
            // Slightly above ground
            Vector3 impactPos = new Vector3(targetPosition.x, 0.6f, targetPosition.z); 
            GameObject impact = Instantiate(impactPrefab, impactPos, Quaternion.identity);

            // X/Z scale based on radius, Y scale thin
            impact.transform.localScale = new Vector3(radius * 2f, 0.3f, radius * 2f);

            Destroy(impact, Duration);
        }

        // Start AoE effect
        nextTickTime = Time.time;
        StartCoroutine(ApplyAoE());
    }

    private IEnumerator ApplyAoE()
    {
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            if (Time.time >= nextTickTime)
            {
                Collider[] hits = Physics.OverlapSphere(targetPosition, radius);
                foreach (var hit in hits)
                {
                    Enemy enemy = hit.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(power);
                        if (isFreezeSpell)
                            enemy.ApplyFreeze(freezeDuration, freezeSlowMultiplier);
                    }
                }

                nextTickTime = Time.time + interval;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject); // finally destroy the spell object after AoE ends
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (hasLanded)
            Gizmos.DrawWireSphere(targetPosition, radius);
    }
}
