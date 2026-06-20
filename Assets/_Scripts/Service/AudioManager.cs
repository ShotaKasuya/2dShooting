using _Scripts.Model;
using _Scripts.UnityServiceLocator;
using UnityEngine;
using UnityEngine.Audio;

namespace _Scripts.Service
{
    /// <summary>
    /// BGMとSEの再生、および音量を管理するサービス。
    /// AudioClipAsset（ScriptableObject）への参照を直接受け取ることで、
    /// 文字列に依存しない型安全な再生を実現する。
    /// </summary>
    public class AudioManager : GlobalService<AudioManager>
    {
        [SerializeField] private AudioMixer audioMixer;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;

        #region Volume Control

        public void SetMasterVolume(float volume) => SetMixerVolume("MasterVolume", volume);
        public void SetBgmVolume(float volume) => SetMixerVolume("BgmVolume", volume);
        public void SetSeVolume(float volume) => SetMixerVolume("SeVolume", volume);

        private void SetMixerVolume(string parameterName, float volume)
        {
            if (audioMixer == null) return;
            // 0.0-1.0 を -80dB-0dB に変換
            float db = volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20f;
            audioMixer.SetFloat(parameterName, db);
        }

        #endregion

        #region BGM Operations

        /// <summary>
        /// BGMを再生する。既に同じアセットが再生中の場合は何もしない。
        /// </summary>
        public void PlayBgm(AudioClipAsset asset)
        {
            if (asset == null || asset.clip == null) return;

            if (bgmSource.clip == asset.clip && bgmSource.isPlaying) return;

            bgmSource.clip = asset.clip;
            bgmSource.volume = asset.volume;
            bgmSource.loop = true; // BGMは基本ループ
            bgmSource.Play();
        }

        public void StopBgm()
        {
            bgmSource.Stop();
        }

        #endregion

        #region SE Operations

        /// <summary>
        /// 効果音を一度だけ再生する。
        /// </summary>
        public void PlaySe(AudioClipAsset asset)
        {
            if (asset == null || asset.clip == null) return;

            seSource.PlayOneShot(asset.clip, asset.volume);
        }

        #endregion
    }
}
