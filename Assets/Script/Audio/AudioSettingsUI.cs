using UnityEngine;
using UnityEngine.UI;

namespace TDGameDev.Audio
{
    public class AudioSettingsUI : MonoBehaviour
    {
        [Header("Sliders (0..1)")]
        public Slider sfxSlider;
        public Slider bgmSlider; // Optional: if you have a BGM manager

        [Header("Managers")]
        public SfxManager sfxManager;
        public MonoBehaviour bgmManagerMono; // Optional reference to your BGM manager script
        public string bgmExposedParam = "BGMVolume"; // Optional exposed param name

        private void Awake()
        {
            // Auto-find SfxManager if not assigned
            if (sfxManager == null) sfxManager = SfxManager.Instance;
        }

        private void OnEnable()
        {
            if (sfxSlider != null)
                sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

            if (bgmSlider != null)
                bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        }

        private void OnDisable()
        {
            if (sfxSlider != null)
                sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);

            if (bgmSlider != null)
                bgmSlider.onValueChanged.RemoveListener(OnBgmSliderChanged);
        }

        public void OnSfxSliderChanged(float value)
        {
            if (sfxManager != null)
            {
                sfxManager.SetVolumeFromSlider(value);
            }
        }

        public void OnBgmSliderChanged(float value)
        {
            // Optional: If you have a BGM manager with an exposed mixer param,
            // you can update it here. This is a placeholder example that tries
            // to find an AudioMixerGroup via SfxManager (shared mixer) and set
            // a BGM exposed parameter.
            if (sfxManager != null && sfxManager.GetMixerGroup() != null && !string.IsNullOrEmpty(bgmExposedParam))
            {
                var mixer = sfxManager.GetMixerGroup().audioMixer;
                if (mixer != null)
                {
                    mixer.SetFloat(bgmExposedParam, SfxManager.LinearToDb(Mathf.Clamp01(value)));
                }
            }
        }
    }
}
