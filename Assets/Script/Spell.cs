using UnityEngine;

public class Spell : MonoBehaviour
{
    public int power = 10;
    public float radius = 2f;
    public float Duration = 2f;

    private void Start()
    {
        ApplyEffect();
        Destroy(gameObject, Duration);
    }

    void ApplyEffect()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(power);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
