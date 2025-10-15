using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    [Header("UI Elements")]
    public GameObject panel;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI roundWaveText;
    public Button retryButton;
    public Button mainMenuButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        panel.SetActive(false);

        retryButton.onClick.AddListener(OnRetryClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void ShowGameOver(int round, int wave, bool playerWon)
    {
        panel.SetActive(true);
        roundWaveText.text = $"Round: {round} | Wave: {wave}";
        messageText.text = playerWon ? "YOU WIN!" : "YOU LOSE!";
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    public void OnRetryClicked()
    {
        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void OnMainMenuClicked()
    {
        // Load your main menu scene
        SceneManager.LoadScene("MainMenu"); // Replace with your actual main menu scene name
    }

    public void OnStartClicked()
    {
        SceneManager.LoadScene("GameStage");
    }

    public void OnExitClicked()
    {
        Debug.Log("Exiting game...");
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in editor
    #else
        Application.Quit(); // Quit build
    #endif
    }

}
