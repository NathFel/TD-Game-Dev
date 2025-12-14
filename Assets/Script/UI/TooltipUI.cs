using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TDGameDev.UI
{
    public class TooltipUI : MonoBehaviour
    {
        [Header("UI References")]
        public RectTransform root;         // Panel root to move/show
        public TMP_Text titleText;
        public TMP_Text bodyText;
        public Image artworkImage;

        [Header("Raycast")]
        public CanvasGroup canvasGroup;    // Should not block raycasts

        [Header("Layout")]
        public Vector2 padding = new Vector2(8, 6);
        public Vector2 offset = new Vector2(18, 24); // top-right of cursor (above the pointer)

        public void SetContent(string title, string body, Sprite artwork)
        {
            if (titleText) titleText.text = title ?? string.Empty;
            if (bodyText) bodyText.text = body ?? string.Empty;
            if (artworkImage)
            {
                artworkImage.sprite = artwork;
                artworkImage.enabled = artwork != null;
            }
        }

        public void SetVisible(bool visible)
        {
            if (root == null) root = GetComponent<RectTransform>();
            if (root) root.gameObject.SetActive(visible);
        }

        public void SetPosition(Vector2 screenPos)
        {
            if (root == null) root = GetComponent<RectTransform>();
            if (root == null) return;
            // Simple screen-space positioning with clamp
            var pos = screenPos + offset;
            pos.x = Mathf.Clamp(pos.x, padding.x, Screen.width - padding.x);
            pos.y = Mathf.Clamp(pos.y, padding.y, Screen.height - padding.y);
            root.position = pos;
        }

        private void Awake()
        {
            if (root == null) root = GetComponent<RectTransform>();
            
            // Set pivot to top-left
            if (root != null)
            {
                root.pivot = new Vector2(0f, 1f);
                
                // Add ContentSizeFitter to auto-size to preferred size
                var fitter = root.GetComponent<ContentSizeFitter>();
                if (fitter == null) fitter = root.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                // Add VerticalLayoutGroup for automatic spacing
                var layout = root.GetComponent<VerticalLayoutGroup>();
                if (layout == null) layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(10, 10, 10, 10);
                layout.spacing = 4;
                layout.childControlWidth = true;
                layout.childControlHeight = true;  // Let it control height
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
            }

            // Setup text wrapping
            if (bodyText != null)
            {
                bodyText.enableWordWrapping = true;
                bodyText.overflowMode = TextOverflowModes.Overflow;
                
                // Add LayoutElement to bodyText to control sizing
                var layoutElem = bodyText.GetComponent<LayoutElement>();
                if (layoutElem == null) layoutElem = bodyText.gameObject.AddComponent<LayoutElement>();
                layoutElem.preferredWidth = 230;
                layoutElem.preferredHeight = -1;  // No preferred height, let it calculate
                layoutElem.flexibleWidth = 0;
                layoutElem.flexibleHeight = -1;   // No flex, just natural size
            }

            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            // Ensure tooltip visuals do not block mouse raycasts
            foreach (var g in GetComponentsInChildren<Graphic>(true))
            {
                g.raycastTarget = false;
            }
        }
    }
}
