using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;
    private int damage;
    private float speed;

    private bool isSeeking;
    private int piercesLeft;
    private int bouncesLeft;
    private float AoERadius;
    private float bounceRadius = 1000f;
    private TowerData dataReference;

    private Vector3 direction;

    public GameObject AoEIndicatorPrefab;

    private float lifetime = 5f;

    public event System.Action<Enemy> onHitEnemy;

    public void SetStats(Transform _target, int _damage, float _speed, TowerData data)
    {
        target = _target;
        damage = _damage;
        speed = _speed;

        if (data != null)
        {
            isSeeking = data.isSeeking;
            piercesLeft = data.pierce;
            bouncesLeft = data.bounce;
            AoERadius = data.AoERadius;
            dataReference = data;
        }

        direction = (target != null) ? (target.position - transform.position).normalized : transform.forward;
    }

    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null && isSeeking)
        {
            Destroy(gameObject);
            return;
        }

        if (isSeeking && target != null)
            direction = (target.position - transform.position).normalized;

        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        transform.LookAt(transform.position + direction);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        Damage(other.transform);

        lifetime = 5f;

        if (piercesLeft > 0)
        {
            piercesLeft--;
            return;
        }

        if (AoERadius > 0f) AoE();

        if (!HandleBounce(other.transform)) Destroy(gameObject);
    }

    void AoE()
    {
        // spawn indicator from tower data
        if (dataReference != null && dataReference.impactPrefab != null)
        {
            GameObject indicator = Instantiate(
                dataReference.impactPrefab,
                new Vector3(transform.position.x, 0.6f, transform.position.z),
                Quaternion.identity
            );

            indicator.transform.localScale = new Vector3(
                AoERadius * 2f,
                0.3f,
                AoERadius * 2f
            );

            Destroy(indicator, .2f);
        }

        // damage enemies
        Collider[] colliders = Physics.OverlapSphere(transform.position, AoERadius);
        foreach (Collider c in colliders)
            if (c.CompareTag("Enemy")) Damage(c.transform);
    }

    void Damage(Transform enemy)
    {
        Enemy e = enemy.GetComponent<Enemy>();
        if (e != null)
        {
            e.TakeDamage(damage);
            onHitEnemy?.Invoke(e);
        }
    }

    bool HandleBounce(Transform lastHit)
    {
        if (bouncesLeft <= 0) return false;

        Collider[] colliders = Physics.OverlapSphere(transform.position, bounceRadius);
        foreach (Collider c in colliders)
        {
            if (c.CompareTag("Enemy") && c.transform != lastHit)
            {
                target = c.transform;
                direction = (target.position - transform.position).normalized;
                bouncesLeft--;
                return true;
            }
        }

        return false;
    }
}
