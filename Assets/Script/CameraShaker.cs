using UnityEngine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance; // Biar bisa dipanggil dari mana aja

    private Vector3 originalPos;
    private float shakeTimer;
    private float shakeAmount;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            // Pindahkan kamera secara acak di dalam bola kecil
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            shakeTimer = 0f;
            transform.localPosition = originalPos; // Balikin ke posisi semula
        }
    }

    // Fungsi ini yang bakal dipanggil sama Musuh
    public void Shake(float duration, float amount)
    {
        shakeTimer = duration;
        shakeAmount = amount;
    }
}