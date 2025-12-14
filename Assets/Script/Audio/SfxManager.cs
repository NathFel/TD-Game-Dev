using UnityEngine;
using UnityEngine.Audio;

namespace TDGameDev.Audio
{
    // Drop this on a singleton GameObject in your scene (e.g., AudioRoot)
    public class SfxManager : MonoBehaviour
    {
        public static SfxManager Instance { get; private set; }

        [Header("Mixer Routing")]
        public AudioMixerGroup sfxMixerGroup; // Assign 'SFX' mixer group here
        [Tooltip("Exposed mixer parameter name controlling SFX volume in dB (e.g., 'SFXVolume')")]
        public string exposedSfxVolumeParam = "SFXVolume";

        [Header("Volume")]
        [Range(0f, 1f)]
        public float masterVolume = 1f; // Global SFX volume scalar

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public float GetVolume() => Mathf.Clamp01(masterVolume);
        public AudioMixerGroup GetMixerGroup() => sfxMixerGroup;

        public void SetVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            // If using an exposed mixer parameter, update it in dB
            if (sfxMixerGroup != null && sfxMixerGroup.audioMixer != null && !string.IsNullOrEmpty(exposedSfxVolumeParam))
            {
                sfxMixerGroup.audioMixer.SetFloat(exposedSfxVolumeParam, LinearToDb(masterVolume));
            }
        }

        public static float LinearToDb(float linear)
        {
            if (linear <= 0.0001f) return -80f; // Unity's lower bound
            return 20f * Mathf.Log10(linear);
        }

        // Convenience for UI sliders: pass slider [0..1]
        public void SetVolumeFromSlider(float sliderValue)
        {
            SetVolume(sliderValue);
        }
    }
}
