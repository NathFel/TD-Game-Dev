using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount)
    {
        textMesh.SetText(damageAmount.ToString());
        
        // Pastikan warna awal tidak transparan
        textColor = textMesh.color;
        textColor.a = 1f; 
        textMesh.color = textColor;

        disappearTimer = 0.5f; // Teks mulai hilang setelah 0.5 detik
        
        // Gerak naik ke atas + acak dikit ke kiri/kanan biar seru
        moveVector = new Vector3(Random.Range(-0.5f, 0.5f), 3f, 0); 
    }

    private void Update()
    {
        // 1. Gerak Melayang ke Atas
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 3f * Time.deltaTime; // Efek gravitasi makin pelan

        // 2. LOGIKA MENGHADAP KAMERA (Ini pengganti komponen tadi)
        if (Camera.main != null)
        {
            // Teks ngikutin rotasi kamera, jadi selalu tegak lurus di layar
            transform.rotation = Camera.main.transform.rotation;
        }

        // 3. Efek Menghilang (Fade Out) perlahan
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}