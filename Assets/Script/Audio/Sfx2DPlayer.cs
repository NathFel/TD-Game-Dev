using UnityEngine;

namespace TDGameDev.Audio
{
    public static class Sfx2DPlayer
    {
        // Plays a 2D one-shot clip independent of the caller's lifetime
        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            var go = new GameObject("OneShot2D_SFX");
            var src = go.AddComponent<AudioSource>();
            src.spatialBlend = 0f; // 2D
            src.playOnAwake = false;
            src.loop = false;
            float master = 1f;
            var mgr = SfxManager.Instance;
            if (mgr != null)
            {
                master = mgr.GetVolume();
                var group = mgr.GetMixerGroup();
                if (group != null) src.outputAudioMixerGroup = group;
            }
            src.volume = Mathf.Clamp01(volume) * master;
            src.clip = clip;
            src.Play();

            Object.Destroy(go, clip.length + 0.1f);
        }
    }
}
