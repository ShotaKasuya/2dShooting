using _Scripts.Service;
using _Scripts.UnityServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Behaviour
{
    /// <summary>
    /// マップ上のステージオブジェクト。
    /// UI Toolkitを使用して進捗を表示する。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MapStage : MonoBehaviour
    {
        [SerializeField] private string sceneName;
        [SerializeField] private float requiredHoldTime = 2f;
        [SerializeField] private float circleRadius = 50f;
        [SerializeField] private float strokeWidth = 10f;
        [SerializeField] private Color circleColor = Color.white;
        [SerializeField] private Color outlineColor = Color.black;
        [SerializeField] private float outlineWidth = 2f;
        [SerializeField] private bool showTrack = true;
        [SerializeField] private Color trackColor = new Color(1, 1, 1, 0.2f);

        private bool m_IsPlayerOverlapping;
        private float m_CurrentHoldTime;
        private bool m_HasTriggered;
        private InputService m_InputService;
        private VisualElement m_ProgressElement;
        private Transform m_PlayerTransform;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;

            // 進捗描画用の要素を作成
            m_ProgressElement = new VisualElement();
            m_ProgressElement.style.position = Position.Absolute;
            float size = circleRadius * 2 + strokeWidth + outlineWidth * 2;
            m_ProgressElement.style.width = size;
            m_ProgressElement.style.height = size;

            // Vector APIを使用して円形を描画
            m_ProgressElement.generateVisualContent += DrawProgress;

            root.Add(m_ProgressElement);
            m_ProgressElement.style.display = DisplayStyle.None;
        }

        private void Start()
        {
            m_InputService = ServiceLocator.Global.Get<InputService>();
        }

        private void Update()
        {
            if (m_HasTriggered) return;

            if (m_IsPlayerOverlapping && m_InputService.ReadFire())
            {
                m_CurrentHoldTime += Time.deltaTime;

                m_ProgressElement.style.display = DisplayStyle.Flex;
                UpdateUIPosition();
                // 再描画を促す
                m_ProgressElement.MarkDirtyRepaint();

                if (m_CurrentHoldTime >= requiredHoldTime)
                {
                    m_HasTriggered = true;
                    ServiceLocator.Global.Get<SceneTransitionUIService>().LoadSceneAsync(sceneName).Forget();
                }
            }
            else
            {
                m_CurrentHoldTime = 0;
                m_ProgressElement.style.display = DisplayStyle.None;
            }
        }

        private void UpdateUIPosition()
        {
            if (m_PlayerTransform == null) return;

            // プレイヤーの世界座標をスクリーン座標に変換
            Vector3 screenPos = Camera.main.WorldToScreenPoint(m_PlayerTransform.position);
            
            // UI Toolkitの座標系（左上原点、Y軸下向き）に変換
            // ScreenPointは左下原点、Y軸上向き
            float offset = circleRadius + strokeWidth / 2 + outlineWidth;
            float uiX = screenPos.x - offset;
            float uiY = Screen.height - screenPos.y - offset;

            m_ProgressElement.style.left = uiX;
            m_ProgressElement.style.top = uiY;
        }

        private void DrawProgress(MeshGenerationContext mgc)
        {
            var painter = mgc.painter2D;
            float progress = Mathf.Clamp01(m_CurrentHoldTime / requiredHoldTime);

            float centerOffset = circleRadius + strokeWidth / 2 + outlineWidth;
            Vector2 center = new Vector2(centerOffset, centerOffset);

            // 1. 背景のトラックを描画
            if (showTrack)
            {
                painter.lineWidth = strokeWidth;
                painter.lineCap = LineCap.Round;
                painter.strokeColor = trackColor;
                painter.BeginPath();
                painter.Arc(center, circleRadius, 0, 360, ArcDirection.Clockwise);
                painter.Stroke();
            }

            if (progress <= 0) return;

            // 12時の方向から時計回りに描画するために -90度から開始
            float startAngle = -90f;
            float endAngle = startAngle + (progress * 360f);

            // 2. 輪郭線を描画 (メインの線より少し太く描く)
            if (outlineWidth > 0)
            {
                painter.lineWidth = strokeWidth + outlineWidth * 2;
                painter.lineCap = LineCap.Round;
                painter.strokeColor = outlineColor;
                painter.BeginPath();
                painter.Arc(center, circleRadius, startAngle, endAngle, ArcDirection.Clockwise);
                painter.Stroke();
            }

            // 3. メインの進捗線を描画
            painter.lineWidth = strokeWidth;
            painter.lineCap = LineCap.Round;
            painter.strokeColor = circleColor;
            painter.BeginPath();
            painter.Arc(center, circleRadius, startAngle, endAngle, ArcDirection.Clockwise);
            painter.Stroke();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                m_IsPlayerOverlapping = true;
                m_PlayerTransform = other.transform;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                m_IsPlayerOverlapping = false;
                m_PlayerTransform = null;
            }
        }
    }
}