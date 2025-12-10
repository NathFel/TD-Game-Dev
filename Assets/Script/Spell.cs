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
    public GameObject impactPrefab; // Efek Ledakan (Instant)
    public GameObject lastingParticlesPrefab; // Efek Area/Lingkaran di tanah (Durasi)
    public Renderer spellRenderer;  // Bola spell yang melayang

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

            // Gerakan Melengkung (Parabolic)
            float height = Mathf.Sin(t * Mathf.PI) * arcHeight;
            transform.position = Vector3.Lerp(startPos, targetPosition, t) + Vector3.up * height;

            yield return null;
        }

        // --- SAAT MENDARAT ---
        transform.position = targetPosition;
        hasLanded = true;

        // 1. Sembunyikan Bola Spell
        if (spellRenderer != null)
            spellRenderer.enabled = false;

        // 2. Spawn IMPACT (Ledakan DUAR!)
        if (impactPrefab != null)
        {
            // Munculkan agak tinggi dikit (y + 1) biar gak ketelen tanah
            Vector3 impactPos = targetPosition + Vector3.up * 1f; 
            GameObject impact = Instantiate(impactPrefab, impactPos, Quaternion.identity);
            
            // Reset transform to avoid inherited scaling/rotation
            impact.transform.rotation = Quaternion.identity;
            impact.transform.localScale = Vector3.one; 

            // Try to play ALL particle systems in the prefab hierarchy
            var systems = impact.GetComponentsInChildren<ParticleSystem>(true);
            if (systems != null && systems.Length > 0)
            {
                // Enable emission and play
                foreach (var ps in systems)
                {
                    var emission = ps.emission;
                    emission.enabled = true;
                    ps.Play(true);
                    // Force a small burst if emission was disabled in the asset
                    ps.Emit(5);
                }

                // Compute a safe lifetime: max of durations + lifetimes
                float maxLifetime = 0f;
                foreach (var ps in systems)
                {
                    var main = ps.main;
                    float duration = main.duration;
                    float lifetime = main.startLifetime.constantMax;
                    maxLifetime = Mathf.Max(maxLifetime, duration + lifetime + 0.5f);
                }
                Destroy(impact, Mathf.Max(2f, maxLifetime));
            }
            else
            {
                // No particle systems found – keep object briefly so we can see it
                Debug.LogWarning("Impact prefab has no ParticleSystem components in root or children.");
                Destroy(impact, 3f);
            }
        }

        // 3. Spawn LASTING PARTICLES (Bekas Area di Tanah)
        if (lastingParticlesPrefab != null)
        {
            Vector3 groundPos = targetPosition + Vector3.up * 0.1f; // Dikit aja di atas tanah
            GameObject particles = Instantiate(lastingParticlesPrefab, groundPos, Quaternion.identity);

            // Sesuaikan lebar partikel dengan Radius Spell
            // (Y dibikin tipis karena nempel tanah)
            particles.transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);

            Destroy(particles, Duration); // Hapus setelah durasi spell habis
        }

        // Mulai logika damage area
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

        Destroy(gameObject); // Hancurkan object spell utama
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (hasLanded)
            Gizmos.DrawWireSphere(targetPosition, radius);
    }
}