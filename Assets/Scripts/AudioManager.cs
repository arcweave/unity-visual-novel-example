using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Arcweave.Project;

namespace Arcweave
{
    /// Handles audio playback driven by AudioAssets attached to Arcweave elements.
    /// Attach to any GameObject in the scene and assign the Player field.
    public class AudioManager : MonoBehaviour
    {
        [Tooltip("The ArcweavePlayer driving the narrative.")]
        public ArcweavePlayer Player;

        [Tooltip("Fade duration in seconds for loop start/stop transitions.")]
        public float fadeDuration = 1.5f;

        [Tooltip("Global volume multiplier applied to all sources.")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;

        // One shared source for fire-and-forget clips.
        private AudioSource _sfxSource;

        // Looping sources keyed by asset ID.
        private readonly Dictionary<string, AudioSource> _loopingSources = new();

        // Target volumes keyed by asset ID (before masterVolume scaling).
        private readonly Dictionary<string, float> _targetVolumes = new();

        // Active fade coroutines keyed by asset ID.
        private readonly Dictionary<string, Coroutine> _fadeCoroutines = new();

        private bool _paused;

        // -------------------------------------------------------------------------

        void Awake()
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
        }

        void OnEnable()
        {
            if (Player != null)
                Player.onElementEnter += OnElementEnter;
        }

        void OnDisable()
        {
            if (Player != null)
                Player.onElementEnter -= OnElementEnter;
        }

        void OnValidate()
        {
            ApplyMasterVolume();
        }

        // -------------------------------------------------------------------------

        private void OnElementEnter(Element element)
        {
            var assets = element.GetAudioAssets();
            if (assets == null || assets.Length == 0) return;

            foreach (var audio in assets)
            {
                switch (audio.mode)
                {
                    case AudioAsset.Mode.Once:
                        StartCoroutine(PlayOnce(audio));
                        break;
                    case AudioAsset.Mode.Loop:
                        StartCoroutine(PlayLoop(audio));
                        break;
                    case AudioAsset.Mode.Stop:
                        FadeOutAndStop(audio.asset);
                        break;
                }
            }
        }

        // -------------------------------------------------------------------------

        private IEnumerator PlayOnce(AudioAsset audio)
        {
            if (audio.delay > 0f)
                yield return new WaitForSeconds(audio.delay);

            AudioClip clip = audio.TryGetAudioClip();
            if (clip != null)
                _sfxSource.PlayOneShot(clip, audio.volume * masterVolume);
        }

        private IEnumerator PlayLoop(AudioAsset audio)
        {
            if (audio.delay > 0f)
                yield return new WaitForSeconds(audio.delay);

            AudioClip clip = audio.TryGetAudioClip();
            if (clip == null) yield break;

            if (!_loopingSources.TryGetValue(audio.asset, out AudioSource source) || source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = true;
                source.volume = 0f;
                _loopingSources[audio.asset] = source;
            }

            if (source.isPlaying && source.clip == clip)
                yield break;

            _targetVolumes[audio.asset] = audio.volume;

            CancelFade(audio.asset);
            source.clip = clip;
            source.volume = 0f;
            source.Play();
            if (_paused) source.Pause();

            _fadeCoroutines[audio.asset] = StartCoroutine(
                FadeVolume(source, 0f, audio.volume * masterVolume, fadeDuration));
        }

        private void FadeOutAndStop(string assetId)
        {
            if (!_loopingSources.TryGetValue(assetId, out AudioSource source) || source == null)
                return;

            CancelFade(assetId);
            _fadeCoroutines[assetId] = StartCoroutine(FadeOutThenDestroy(source, assetId, source.volume));
        }

        // -------------------------------------------------------------------------

        /// <summary>Fade out and stop all looping sources.</summary>
        [ContextMenu("Stop All (Fade)")]
        public void StopAll()
        {
            foreach (var kvp in new Dictionary<string, AudioSource>(_loopingSources))
                FadeOutAndStop(kvp.Key);
            _sfxSource.Stop();
        }

        /// <summary>Stop all looping sources immediately, without fade.</summary>
        [ContextMenu("Stop All Immediate")]
        public void StopAllImmediate()
        {
            foreach (var kvp in new Dictionary<string, AudioSource>(_loopingSources))
            {
                CancelFade(kvp.Key);
                if (kvp.Value != null) { kvp.Value.Stop(); Destroy(kvp.Value); }
            }
            _loopingSources.Clear();
            _targetVolumes.Clear();
            _sfxSource.Stop();
        }

        /// <summary>Pause all looping sources.</summary>
        [ContextMenu("Pause All")]
        public void PauseAll()
        {
            if (_paused) return;
            _paused = true;
            foreach (var source in _loopingSources.Values)
                if (source != null) source.Pause();
        }

        /// <summary>Resume all paused looping sources.</summary>
        [ContextMenu("Resume All")]
        public void ResumeAll()
        {
            if (!_paused) return;
            _paused = false;
            foreach (var source in _loopingSources.Values)
                if (source != null) source.UnPause();
        }

        /// <summary>Set the master volume and apply it to all active sources immediately.</summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyMasterVolume();
        }

        // -------------------------------------------------------------------------

        private void ApplyMasterVolume()
        {
            foreach (var kvp in _loopingSources)
            {
                if (kvp.Value == null) continue;
                float target = _targetVolumes.TryGetValue(kvp.Key, out float t) ? t : 1f;
                kvp.Value.volume = target * masterVolume;
            }
        }

        private IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (source == null) yield break;
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            if (source != null) source.volume = to;
        }

        private IEnumerator FadeOutThenDestroy(AudioSource source, string assetId, float fromVolume)
        {
            yield return FadeVolume(source, fromVolume, 0f, fadeDuration);
            if (source != null) { source.Stop(); Destroy(source); }
            _loopingSources.Remove(assetId);
            _targetVolumes.Remove(assetId);
            _fadeCoroutines.Remove(assetId);
        }

        private void CancelFade(string assetId)
        {
            if (_fadeCoroutines.TryGetValue(assetId, out Coroutine c) && c != null)
            {
                StopCoroutine(c);
                _fadeCoroutines.Remove(assetId);
            }
        }

        // -------------------------------------------------------------------------

        /// Read-only state exposed to the custom editor.
        public IReadOnlyDictionary<string, AudioSource> ActiveSources => _loopingSources;
        public bool IsPaused => _paused;
    }
}
