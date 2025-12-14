using UnityEngine;
using TDGameDev.UI;

namespace TDGameDev.UI
{
    public class TooltipManager : MonoBehaviour
    {
        public static TooltipManager Instance { get; private set; }

        [Header("References")]
        public TooltipUI tooltipUI;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (tooltipUI != null)
                tooltipUI.SetVisible(false);
        }

        public void Show(Card card)
        {
            if (tooltipUI == null || card == null) return;

            // Show only the description; no name or stat lines
            string body = card.description;

            // If description is empty/whitespace, do not show tooltip
            if (string.IsNullOrWhiteSpace(body))
            {
                Hide();
                return;
            }
            tooltipUI.SetContent(string.Empty, body, card.artwork);
            tooltipUI.SetVisible(true);
            // Position will be updated in Update() based on mouse
        }

        public void Hide()
        {
            if (tooltipUI == null) return;
            tooltipUI.SetVisible(false);
        }

        private void Update()
        {
            if (tooltipUI == null) return;
            if (!tooltipUI.gameObject.activeSelf) return;

            var mousePos = Input.mousePosition;
            tooltipUI.SetPosition(mousePos);
        }
    }
}
