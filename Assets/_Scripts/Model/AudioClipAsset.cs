using UnityEngine;

namespace _Scripts.Model
{
    /// <summary>
    /// 単一の音声アセット（AudioClipとその設定）を保持するScriptableObject。
    /// 文字列キーに依存せず、このアセットへの参照を直接渡すことで再生を行う。
    /// </summary>
    [CreateAssetMenu(fileName = "NewAudioClipAsset", menuName = "Shooting2D/AudioClipAsset")]
    public class AudioClipAsset : ScriptableObject
    {
        public AudioClip clip;
        [Range(0, 1)] public float volume = 1f;
        public bool loop = false;
    }
}
