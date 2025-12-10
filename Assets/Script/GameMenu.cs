using UnityEngine;
using UnityEngine.UI; // Wajib ada buat UI

public class GameMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;   // Masukkan Panel Menu di sini
    public Slider volumeSlider;     // Masukkan Slider Volume di sini

    [Header("Audio References")]
    public AudioSource musicSource; // Masukkan Object yang muter lagu background di sini

    private bool isPaused = false;

    void Start()
    {
        // Sembunyikan menu saat game mulai
        if (pausePanel != null) 
        {
            pausePanel.SetActive(false);
        }

        // Kalau ada settingan volume slider, samain posisinya dengan volume asli lagunya
        if (musicSource != null && volumeSlider != null)
        {
            volumeSlider.value = musicSource.volume;
            
            // Kode ini biar slidernya otomatis manggil fungsi SetVolume pas digeser
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    void Update()
    {
        // Tombol ESC buat Pause/Resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true); // Munculin menu
        Time.timeScale = 0f;        // Stop waktu (Game berhenti)
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false); // Umpetin menu
        Time.timeScale = 1f;         // Jalanin waktu lagi
        isPaused = false;
    }

    // Fungsi ini dipanggil otomatis sama Slider
    public void SetVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }
    
    // Fungsi buat tombol Quit (Opsional)
    public void QuitGame()
    {
        Debug.Log("Keluar dari Game!");

        // Tambahan kode: Biar tombolnya bisa stop mode Play di Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

        // Kode asli: Ini yang jalan pas udah jadi Game beneran
        Application.Quit();
    }
}