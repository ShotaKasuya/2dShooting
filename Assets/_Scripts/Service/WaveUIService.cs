using System.Threading;
using _Scripts.Model;
using _Scripts.UnityServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Service
{
    /// <summary>
    /// Wave進行に関するUI演出を制御するクラス。
    /// UI Toolkit (UXML/USS) を使用し、アニメーションのトリガーと状態管理を行う。
    /// ServiceLocator経由で提供される。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class WaveUIService : MonoBehaviour
    {
        [SerializeField] private AudioClipAsset waveStartSound;
        private VisualElement m_Root;
        private Label m_LblWaveStart;
        private Label m_LblWaveClear;
        private Label m_LblCountdown;
        private AudioManager m_AudioManager;

        private void Awake()
        {
            ServiceLocator.ForSceneOf(this).Register(this);
        }

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;

            m_Root = uiDocument.rootVisualElement;
            m_LblWaveStart = m_Root.Q<Label>("lbl-wave-start");
            m_LblWaveClear = m_Root.Q<Label>("lbl-wave-clear");
            m_LblCountdown = m_Root.Q<Label>("lbl-countdown");

            ServiceLocator.Global.TryGet(out m_AudioManager);
            Debug.Assert(m_AudioManager != null, "AudioManager not found in Global ServiceLocator.", this);
            
            ResetLabels();
        }

        /// <summary>
        /// 全てのラベルの状態を初期状態（非表示）に戻す。
        /// </summary>
        private void ResetLabels()
        {
            if (m_LblWaveStart != null)
            {
                m_LblWaveStart.ClearClassList();
                m_LblWaveStart.AddToClassList("wave-label");
                m_LblWaveStart.AddToClassList("wave-start-hidden");
                m_LblWaveStart.text = "";
            }

            if (m_LblWaveClear != null)
            {
                m_LblWaveClear.ClearClassList();
                m_LblWaveClear.AddToClassList("wave-label");
                m_LblWaveClear.AddToClassList("wave-clear-hidden");
                m_LblWaveClear.text = "WAVE CLEAR";
            }

            if (m_LblCountdown != null)
            {
                m_LblCountdown.ClearClassList();
                m_LblCountdown.AddToClassList("wave-label");
                m_LblCountdown.AddToClassList("countdown-label");
                m_LblCountdown.AddToClassList("countdown-hidden");
                m_LblCountdown.text = "";
            }
        }

        /// <summary>
        /// Wave開始演出を表示する。
        /// </summary>
        /// <param name="wave">Wave番号</param>
        public void ShowWaveStart(int wave)
        {
            ShowWaveStartAsync(wave, destroyCancellationToken).Forget();
        }

        public async UniTask ShowWaveStartAsync(int wave, CancellationToken token)
        {
            ResetLabels();
            m_LblWaveStart.text = $"WAVE {wave}";

            if (waveStartSound != null)
            {
                m_AudioManager.PlaySe(waveStartSound);
            }
            
            // 1. 初期状態 (wave-start-hidden)
            
            // 2. 0.25秒で拡大(1.15)とフェードイン
            m_LblWaveStart.RemoveFromClassList("wave-start-hidden");
            m_LblWaveStart.AddToClassList("wave-start-pop");
            await UniTask.Delay(250, cancellationToken: token);
            
            // 3. 0.2秒で等倍(1.0)へ収束
            m_LblWaveStart.RemoveFromClassList("wave-start-pop");
            m_LblWaveStart.AddToClassList("wave-start-idle");
            await UniTask.Delay(200, cancellationToken: token);
            
            // 4. 1.0秒間停止
            await UniTask.Delay(1000, cancellationToken: token);
            
            // 5. 0.4秒で上方向へ移動しながらフェードアウト
            m_LblWaveStart.RemoveFromClassList("wave-start-idle");
            m_LblWaveStart.AddToClassList("wave-start-exit");
            await UniTask.Delay(400, cancellationToken: token);
            
            // 6. 完全に非表示 (Reset)
            ResetLabels();
        }

        /// <summary>
        /// Wave完了演出を表示する。
        /// </summary>
        public void ShowWaveClear()
        {
            ShowWaveClearAsync(destroyCancellationToken).Forget();
        }

        public async UniTask ShowWaveClearAsync(CancellationToken token)
        {
            ResetLabels();
            
            // フェードイン & パンチスケール(1.1)
            m_LblWaveClear.RemoveFromClassList("wave-clear-hidden");
            m_LblWaveClear.AddToClassList("wave-clear-show");
            await UniTask.Delay(200, cancellationToken: token);
            
            // 等倍へ
            m_LblWaveClear.RemoveFromClassList("wave-clear-show");
            m_LblWaveClear.AddToClassList("wave-clear-idle");
            
            // 1.5秒間表示
            await UniTask.Delay(1500, cancellationToken: token);
            
            // フェードアウト
            m_LblWaveClear.AddToClassList("wave-clear-hidden");
            await UniTask.Delay(300, cancellationToken: token);
            
            ResetLabels();
        }

        /// <summary>
        /// 次Waveへのカウントダウンを表示する。
        /// </summary>
        /// <param name="seconds">秒数</param>
        public void ShowCountdown(int seconds)
        {
            ShowCountdownAsync(seconds, destroyCancellationToken).Forget();
        }

        public async UniTask ShowCountdownAsync(int seconds, CancellationToken token)
        {
            ResetLabels();
            
            m_LblCountdown.RemoveFromClassList("countdown-hidden");
            m_LblCountdown.AddToClassList("countdown-visible");
            
            for (int i = seconds; i >= 1; i--)
            {
                m_LblCountdown.text = $"NEXT WAVE IN {i}";
                
                // 色とスケールの状態更新
                m_LblCountdown.RemoveFromClassList("countdown-normal");
                m_LblCountdown.RemoveFromClassList("countdown-warning");
                m_LblCountdown.RemoveFromClassList("countdown-critical");
                
                if (i > 3)
                {
                    m_LblCountdown.AddToClassList("countdown-normal");
                }
                else if (i > 1)
                {
                    m_LblCountdown.AddToClassList("countdown-warning");
                }
                else
                {
                    // 残り1秒で大きく強調
                    m_LblCountdown.AddToClassList("countdown-critical");
                }
                
                // 毎秒のスケールアニメーション(Tick)
                m_LblCountdown.AddToClassList("countdown-tick");
                
                // 1秒待機しつつTick演出を戻す
                await UniTask.Delay(150, cancellationToken: token);
                m_LblCountdown.RemoveFromClassList("countdown-tick");
                await UniTask.Delay(850, cancellationToken: token);
            }
            
            // 非表示
            m_LblCountdown.RemoveFromClassList("countdown-visible");
            m_LblCountdown.AddToClassList("countdown-hidden");
            await UniTask.Delay(150, cancellationToken: token);
            ResetLabels();
        }
    }
}