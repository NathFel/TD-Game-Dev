using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    private Transform target;

    [Header("References")]
    public Transform partToRotate;
    public Transform firePoint;
    public LineRenderer lineRenderer;
    public ParticleSystem impactEffect;

    [Header("Turret Settings")]
    public TargetingMode targetingMode = TargetingMode.Closest;
    public float yRotationOffset = 90f;
    public Animator animator;

    [Header("Card Settings")]
    public Card turretCard;
    private float fireCountdown = 0f;

    [Header("Selection Settings")]
    public GameObject rangeSpherePrefab; // Assign a transparent sphere prefab
    private GameObject rangeSphereInstance;
    private bool isSelected = false;

    private Node node; 
    private GameObject currentPreview;
    private List<Transform> waypoints = new List<Transform>();
    private List<Transform> reversedRoute = new List<Transform>();

    private void Start()
    {
        node = GetComponentInParent<Node>(); 

        GameObject wpRoot = GameObject.Find("waypoints");

        if (wpRoot != null)
        {
            foreach (Transform child in wpRoot.transform)
                waypoints.Add(child);
            
            reversedRoute = new List<Transform>(waypoints);
            reversedRoute.Reverse();
        }
        else
        {
            Debug.LogWarning("No GameObject named 'Waypoints' found in scene!");
        }
    }

    private void OnMouseEnter()
    {
        if (node == null) return;

        // Remove previous preview if any
        if (currentPreview != null) Destroy(currentPreview);

        Vector3 spawnPos = node.transform.position;

        // Use Node prefab and Y offsets
        if (node.HasObject() && node.previewOccupiedPrefab != null)
        {
            spawnPos.y += node.occupiedYOffset;
            currentPreview = Instantiate(node.previewOccupiedPrefab, spawnPos, Quaternion.identity);
        }
        else if (!node.HasObject() && node.previewEmptyPrefab != null)
        {
            spawnPos.y += node.emptyYOffset;
            currentPreview = Instantiate(node.previewEmptyPrefab, spawnPos, Quaternion.identity);
        }
    }

    private void OnMouseExit()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }

    public enum TargetingMode
    {
        TargetFirst,
        HighestHP,
        Closest,
        TargetLast
    }

    void Update()
    {
        HandleSelectionVisual();

        if (turretCard == null || turretCard.towerData == null) return;

        if (turretCard.towerData.isSummoner)
        {
            HandleSummoner();
            return; 
        }

        UpdateTarget();
        
        if (target == null)
        {
            DisableLaser();
            return;
        }

        LockOnTarget();

        if (turretCard.towerData.useLaser)
        {
            Laser();
        }
        else
        {
            fireCountdown -= Time.deltaTime;
            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = 1f / Mathf.Max(turretCard.towerData.fireRate, 0.01f);
            }
        }
    }

    private void HandleSelectionVisual()
    {
        if (isSelected)
        {
            if (rangeSphereInstance == null && rangeSpherePrefab != null)
            {
                // Offset Y by 5 units
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                rangeSphereInstance = Instantiate(rangeSpherePrefab, spawnPos, Quaternion.identity);

                float diameter = turretCard.towerData.range * 2f;
                rangeSphereInstance.transform.localScale = new Vector3(diameter, 0.1f, diameter);
            }
        }
        else
        {
            if (rangeSphereInstance != null)
            {
                Destroy(rangeSphereInstance);
            }
        }

        
    }

    public void Select()
    {
        isSelected = true;
        TurretInfoUI.Instance?.ShowTurretInfo(this);
    }

    public void Deselect()
    {
        isSelected = false;
        TurretInfoUI.Instance?.Hide();
    }

    private void OnDestroy()
    {
        if (rangeSphereInstance != null)
        {
            Destroy(rangeSphereInstance);
        }
    }

    void UpdateTarget()
    {
        List<Enemy> enemies = EnemyWaveManager.Instance.activeEnemies;
        if (enemies.Count == 0)
        {
            target = null;
            return;
        }

        Enemy chosen = null;
        float bestValue = (targetingMode == TargetingMode.Closest || targetingMode == TargetingMode.TargetLast)
            ? Mathf.Infinity
            : -Mathf.Infinity;

        float rangeSqr = turretCard.towerData.range * turretCard.towerData.range;

        foreach (Enemy e in enemies)
        {
            if (e == null) continue;

            float distanceSqr = (e.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr > rangeSqr) continue;

            float value = targetingMode switch
            {
                TargetingMode.Closest => distanceSqr,
                TargetingMode.HighestHP => e.Health,
                TargetingMode.TargetFirst => e.GetPathProgress(),
                TargetingMode.TargetLast => e.GetPathProgress(),
                _ => 0f
            };

            bool isBetter = targetingMode switch
            {
                TargetingMode.Closest => value < bestValue,
                TargetingMode.TargetLast => value < bestValue,
                TargetingMode.HighestHP => value > bestValue,
                TargetingMode.TargetFirst => value > bestValue,
                _ => false
            };

            if (isBetter)
            {
                bestValue = value;
                chosen = e;
            }
        }

        target = chosen != null ? chosen.transform : null;
    }

    void LockOnTarget()
    {
        if (target == null || partToRotate == null) return;

        Vector3 dir = target.position - partToRotate.position;
        if (dir == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, yRotationOffset, 0f);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turretCard.towerData.turnSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    void Shoot()
    {
        if (turretCard.bulletPrefab == null) return;

        if (animator != null)
            animator.SetTrigger("Launch");

        GameObject bulletGO = Instantiate(turretCard.bulletPrefab, firePoint.position, firePoint.rotation);

        // CATAPULT MODE
        if (turretCard.towerData.isCatapult)
        {
            CatapultBullet cb = bulletGO.GetComponent<CatapultBullet>();
            cb.Setup(turretCard.towerData);
            cb.onHitEnemy += (Enemy e) => TryApplyBurn(e);
            cb.Launch(target.position);

            return;
        }

        // NORMAL BULLET
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetStats(target, turretCard.towerData.baseDamage, turretCard.towerData.bulletSpeed, turretCard.towerData);
            bullet.onHitEnemy += (Enemy e) => TryApplyBurn(e);
        }
    }

    void HandleSummoner()
    {
        if (EnemyWaveManager.Instance.activeEnemies.Count <= 0) return;

        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f)
        {
            // Request global manager to spawn
            SummonManager.Instance.RequestSummon(SummonMinion);

            // reset turret's own fire countdown
            fireCountdown = 1f / Mathf.Max(turretCard.towerData.fireRate, 0.01f);
        }
    }
    

    void SummonMinion()
    {
        if (reversedRoute == null || reversedRoute.Count == 0) return;

        Transform start = reversedRoute[0];

        Vector3 spawnPos =
            start.position +
            Vector3.up * turretCard.towerData.minionSpawnYOffset;

        GameObject go = Instantiate(
            turretCard.towerData.minionPrefab,
            spawnPos,
            Quaternion.identity
        );

        MinionUnit unit = go.GetComponent<MinionUnit>();
        unit.hp = turretCard.towerData.baseDamage;
        unit.moveSpeed = turretCard.towerData.minionSpeed;

        unit.SetRoute(reversedRoute, turretCard.towerData.minionSpawnYOffset);
    }

    void Laser()
    {
        if (!lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
            impactEffect.Play();
        }

        Quaternion laserRotation = Quaternion.Euler(0f, -90f, 0f) * firePoint.rotation;
        Vector3 laserDirVisual = laserRotation * Vector3.forward;
        float laserLengthVisual = turretCard?.towerData.laserLength ?? 10f;
        float laserWidth = turretCard?.towerData.laserWidth ?? 0.5f;
        Vector3 laserEnd = firePoint.position + laserDirVisual * laserLengthVisual;

        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, laserEnd);
        impactEffect.transform.position = laserEnd;

        float laserDamagePerSecond = turretCard.towerData.baseDamage;

        Vector3 boxCenter = firePoint.position + laserDirVisual * (laserLengthVisual / 2f);
        Vector3 boxHalfExtents = new Vector3(laserWidth / 2f, laserWidth / 2f, laserLengthVisual / 2f);
        Quaternion boxRotation = Quaternion.LookRotation(laserDirVisual);

        Collider[] hits = Physics.OverlapBox(boxCenter, boxHalfExtents, boxRotation);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy e = hit.GetComponent<Enemy>();
                if (e != null)
                {
                    e.TakeDamage(Mathf.FloorToInt(laserDamagePerSecond * Time.deltaTime));
                    TryApplyBurn(e);
                }
            }
        }
    }

    void TryApplyBurn(Enemy e)
    {
        if (turretCard.towerData.burnChance <= 0f) return;

        if (Random.value <= turretCard.towerData.burnChance)
        {
            e.ApplyBurn(turretCard.towerData.burnDamage);
        }
    }

    void DisableLaser()
    {
        if (turretCard.towerData.useLaser && lineRenderer.enabled)
        {
            lineRenderer.enabled = false;
            impactEffect.Stop();
        }
    }
}
