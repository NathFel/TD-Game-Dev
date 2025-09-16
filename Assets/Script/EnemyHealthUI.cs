using UnityEngine;
using TMPro;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("Health Bar")]
    public GameObject healthBarPrefab;
    public Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    public float fixedHealthBarScale = 1f;

    [Header("HP Text (Optional)")]
    public TextMeshPro hpTextPrefab;
    public Vector3 hpTextOffset = new Vector3(0, 0.3f, 0);

    private Transform healthBarInstance;
    private Transform healthBarFill;
    private TextMeshPro hpTextInstance;
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

        // Spawn TMP HP Text
        if (hpTextPrefab != null)
        {
            hpTextInstance = Instantiate(hpTextPrefab, transform.position + healthBarOffset + hpTextOffset, Quaternion.identity, transform);
            hpTextInstance.fontSize = 3;
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            Quaternion cameraRotation = Quaternion.LookRotation(mainCamera.transform.forward);

            if (healthBarInstance != null)
                healthBarInstance.rotation = cameraRotation;

            if (hpTextInstance != null)
                hpTextInstance.transform.rotation = cameraRotation;
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

        if (hpTextInstance != null)
            hpTextInstance.text = currentHealth.ToString();
    }
}
