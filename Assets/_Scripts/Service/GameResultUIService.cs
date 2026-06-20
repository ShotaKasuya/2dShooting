using _Scripts.Behaviour;
using _Scripts.UnityServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace _Scripts.Service
{
    /// <summary>
    /// ゲームの終了結果（勝利・敗北）画面を制御するクラス。
    /// UI Toolkit (UXML/USS) を使用し、表示状態やボタンイベントを管理する。
    /// ServiceLocator経由で提供される。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GameResultUIService : SceneService<GameResultUIService>
    {
        private VisualElement m_Root;
        private Label m_LblMessage;
        private Button m_BtnRetry;
        private Button m_BtnLastWave;
        private Button m_BtnMap;

        private void Awake()
        {
            ServiceLocator.ForSceneOf(this).Register(this);
        }

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;

            // UXML内のルート要素を取得
            m_Root = uiDocument.rootVisualElement.Q<VisualElement>("root");
            if (m_Root == null) return;

            m_LblMessage = m_Root.Q<Label>("lbl-message");
            m_BtnRetry = m_Root.Q<Button>("btn-retry");
            m_BtnLastWave = m_Root.Q<Button>("btn-last-wave");
            m_BtnMap = m_Root.Q<Button>("btn-map");

            // ボタンイベントの登録
            if (m_BtnRetry != null) m_BtnRetry.clicked += OnRetryClicked;
            if (m_BtnLastWave != null) m_BtnLastWave.clicked += OnLastWaveClicked;
            if (m_BtnMap != null) m_BtnMap.clicked += OnMapClicked;

            // 実行時のみ初期状態を非表示にする
            if (Application.isPlaying)
            {
                Hide();
            }
        }

        private void OnDisable()
        {
            if (m_BtnRetry != null) m_BtnRetry.clicked -= OnRetryClicked;
            if (m_BtnLastWave != null) m_BtnLastWave.clicked -= OnLastWaveClicked;
            if (m_BtnMap != null) m_BtnMap.clicked -= OnMapClicked;
        }

        /// <summary>
        /// リザルト画面を非表示にする。
        /// </summary>
        public void Hide()
        {
            m_Root.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// 勝利時の画面を表示する。
        /// </summary>
        public void ShowVictory()
        {
            m_Root.style.display = DisplayStyle.Flex;
            m_LblMessage.text = "VICTORY";

            m_LblMessage.RemoveFromClassList("defeat");
            m_LblMessage.AddToClassList("victory");

            // 勝利時は「直前のWaveから」は不要かもしれない（要件に応じて変更）
            if (m_BtnLastWave != null)
            {
                m_BtnLastWave.style.display = DisplayStyle.None;
            }

            // ゲームパッド対応: 最初のボタンにフォーカスを当てる
            m_BtnRetry.Focus();
        }

        /// <summary>
        /// 敗北時の画面を表示する。
        /// </summary>
        public void ShowDefeat()
        {
            m_Root.style.display = DisplayStyle.Flex;
            m_LblMessage.text = "DEFEAT";

            m_LblMessage.RemoveFromClassList("victory");
            m_LblMessage.AddToClassList("defeat");

            if (m_BtnLastWave != null)
            {
                m_BtnLastWave.style.display = DisplayStyle.Flex;
            }

            // ゲームパッド対応: 最初のボタンにフォーカスを当てる
            m_BtnRetry.Focus();
        }

        private void OnRetryClicked()
        {
            Debug.Log("Retry Clicked");
            ServiceLocator.Global.Get<SceneTransitionUIService>().LoadSceneAsync(SceneManager.GetActiveScene().name).Forget();
        }

        private void OnLastWaveClicked()
        {
            Debug.Log("Last Wave Clicked");
            ServiceLocator.ForSceneOf(this).Get<GameFlowController>();
        }

        private void OnMapClicked()
        {
            Debug.Log("Back to Map Clicked");
            ServiceLocator.Global.Get<SceneTransitionUIService>().LoadSceneAsync("Map").Forget();
        }
    }
}