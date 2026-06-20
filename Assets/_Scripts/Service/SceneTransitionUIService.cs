using Cysharp.Threading.Tasks;
using _Scripts.UnityServiceLocator;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace _Scripts.Service
{
    /// <summary>
    /// シーン遷移時の演出（オーバーレイ）を管理するサービス。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SceneTransitionUIService : GlobalService<SceneTransitionUIService>
    {
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private Color overlayColor = Color.black;
        [SerializeField] private Sprite splashImage;

        private VisualElement m_Overlay;
        private bool m_IsTransitioning;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;

            m_Overlay = new VisualElement();
            m_Overlay.style.position = Position.Absolute;
            m_Overlay.style.left = 0;
            m_Overlay.style.top = 0;
            m_Overlay.style.right = 0;
            m_Overlay.style.bottom = 0;
            m_Overlay.style.backgroundColor = overlayColor;
            m_Overlay.style.opacity = 0;
            m_Overlay.style.display = DisplayStyle.None;

            if (splashImage != null)
            {
                m_Overlay.style.backgroundImage = new StyleBackground(splashImage);
                // 画像のアスペクト比を維持して中央に配置 (Obsolete 対応)
                m_Overlay.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                m_Overlay.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                m_Overlay.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            }

            root.Add(m_Overlay);
        }

        /// <summary>
        /// 画面を覆い、シーンを非同期で読み込む。
        /// </summary>
        public async UniTask LoadSceneAsync(string sceneName)
        {
            m_IsTransitioning = true;
            try
            {
                // 1. 画面を覆い始める
                m_Overlay.style.display = DisplayStyle.Flex;
                await FadeAsync(1, fadeInDuration);

                // 2. シーンを読み込む
                var op = SceneManager.LoadSceneAsync(sceneName);
                await op.ToUniTask();

                // 3. 画面の覆いを解除する
                await FadeAsync(0, fadeOutDuration);
                m_Overlay.style.display = DisplayStyle.None;
            }
            finally
            {
                m_IsTransitioning = false;
            }
        }

        /// <summary>
        /// 遷移（フェードアウト）が完了するまで待機する。
        /// </summary>
        public async UniTask WaitUntilTransitionFinishedAsync()
        {
            await UniTask.WaitWhile(() => m_IsTransitioning);
        }

        private async UniTask FadeAsync(float targetOpacity, float duration)
        {
            float startOpacity = m_Overlay.resolvedStyle.opacity;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                m_Overlay.style.opacity = Mathf.Lerp(startOpacity, targetOpacity, elapsed / duration);
                await UniTask.Yield();
            }

            m_Overlay.style.opacity = targetOpacity;
        }
    }
}
