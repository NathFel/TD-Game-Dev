using UnityEngine;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("Health Bar")]
    public GameObject healthBarPrefab;
    public Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    public float fixedHealthBarScale = 1f;

    private Transform healthBarInstance;
    private Transform healthBarFill;
    private Camera mainCamera;

    private int maxHealth;

    void Awake()
    {
        mainCamera = Camera.main;

        // Spawn Health Bar
        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity, transform);
            healthBarInstance = hb.transform;
            healthBarFill = healthBarInstance.Find("Fill");
            if (healthBarInstance != null)
                healthBarInstance.localScale = Vector3.one * fixedHealthBarScale;
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null && healthBarInstance != null)
        {
            Quaternion cameraRotation = Quaternion.LookRotation(mainCamera.transform.forward);
            healthBarInstance.rotation = cameraRotation;
        }
    }

    public void SetMaxHealth(int health)
    {
        if (health <= 0) health = 1;
        maxHealth = health;
        UpdateHealth(maxHealth);
    }

    public void UpdateHealth(int currentHealth)
    {
        float healthPercent = Mathf.Clamp01((float)currentHealth / maxHealth);

        if (healthBarFill != null)
        {
            Vector3 fillScale = healthBarFill.localScale;
            fillScale.x = healthPercent;
            healthBarFill.localScale = fillScale;
        }
    }
}
