using UnityEngine;
using System.Collections;

// Memastikan object ini punya AudioSource
[RequireComponent(typeof(AudioSource))]
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

    [Header("Audio SFX")]
    public AudioClip launchSFX;     // Suara saat ditembak (Whoosh)
    public AudioClip impactSFX;     // Suara saat meledak (Boom)
    public AudioClip aoeLoopSFX;    // Suara area (Burning/Freezing loop)
    
    [Range(0f, 1f)] public float loopVolume = 0.3f; // Volume untuk loop (default 0.3)
    
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Launch(Vector3 start, Vector3 target)
    {
        transform.position = start;
        targetPosition = target;
        nextTickTime = 0f;

        // --- AUDIO: Play Launch Sound ---
        if (launchSFX != null)
        {
            audioSource.PlayOneShot(launchSFX);
        }

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

        // --- AUDIO: Play Impact Sound ---
        if (impactSFX != null)
        {
            // Menggunakan PlayOneShot agar bisa menumpuk dengan suara launch jika belum selesai
            audioSource.PlayOneShot(impactSFX);
        }

        // 1. Sembunyikan Bola Spell
        if (spellRenderer != null)
        {
            spellRenderer.enabled = false;
        }
        else
        {
            var fallbackRenderer = GetComponentInChildren<Renderer>();
            if (fallbackRenderer != null) fallbackRenderer.enabled = false;
        }

        // 2. Spawn IMPACT (Visual Ledakan)
        if (impactPrefab != null)
        {
            Vector3 impactPos = targetPosition + Vector3.up * 1f; 
            GameObject impact = Instantiate(impactPrefab, impactPos, Quaternion.identity);
            
            impact.transform.rotation = Quaternion.identity;
            impact.transform.localScale = Vector3.one; 

            var systems = impact.GetComponentsInChildren<ParticleSystem>(true);
            if (systems != null && systems.Length > 0)
            {
                foreach (var ps in systems)
                {
                    var emission = ps.emission;
                    emission.enabled = true;
                    ps.Play(true);
                    ps.Emit(5);
                }
                Destroy(impact, 3f); // Simplified destroy logic for readability
            }
            else
            {
                 // Handle CFXR or generic destroy
                 Destroy(impact, 3f);
            }
        }

        // 3. Spawn LASTING PARTICLES (Bekas Area di Tanah)
        if (lastingParticlesPrefab != null)
        {
            Vector3 groundPos = targetPosition + Vector3.up * 0.1f;
            GameObject particles = Instantiate(lastingParticlesPrefab, groundPos, Quaternion.identity);
            particles.transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);
            Destroy(particles, Duration);
        }

        // Mulai logika damage area
        nextTickTime = Time.time;
        StartCoroutine(ApplyAoE());
    }

    private IEnumerator ApplyAoE()
    {
        // --- AUDIO: Play Loop Sound (Jika ada) ---
        if (aoeLoopSFX != null)
        {
            audioSource.clip = aoeLoopSFX;
            audioSource.loop = true; // Set agar mengulang terus
            audioSource.volume = loopVolume; // Atur volume sesuai setting
            audioSource.Play();
        }

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
            
            // --- AUDIO: Fade out volume di akhir durasi (smooth transition) ---
            if (aoeLoopSFX != null && elapsed > Duration - 0.5f)
            {
                audioSource.volume = Mathf.Lerp(loopVolume, 0f, (elapsed - (Duration - 0.5f)) / 0.5f);
            }
            
            yield return null;
        }

        Destroy(gameObject); // Hancurkan object spell utama (suara loop akan mati otomatis)
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (hasLanded)
            Gizmos.DrawWireSphere(targetPosition, radius);
    }
}