using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UIElements;

namespace _Scripts.Service
{
    /// <summary>
    /// UI Toolkit と Input System を使用した、モバイル向けの画面上バーチャルスティック（On-Screen Stick）。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class UiTkOnScreenStick : OnScreenControl
    {
        [InputControl(layout = "Vector2")] [SerializeField]
        private string m_ControlPath = "<Gamepad>/leftStick";

        [Header("UI Element Names")] [SerializeField]
        private string backgroundName = "stick-background";

        [SerializeField] private string thumbName = "stick-thumb";

        [Header("Stick Settings")] [Tooltip("スティックの最大可動半径 (ピクセル単位)")] [SerializeField]
        private float movementRange = 100f;

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }

        private VisualElement m_Background;
        private VisualElement m_Thumb;

        private bool m_IsDragging;
        private Vector2 m_StartPointerPosition;

        private void Awake()
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;

            var root = uiDocument.rootVisualElement;
            m_Background = root.Q<VisualElement>(backgroundName);
            m_Thumb = root.Q<VisualElement>(thumbName);

            if (m_Background == null || m_Thumb == null)
            {
                Debug.LogError(
                    $"UIToolkitOnScreenStick: UI Elements not found. Background: {backgroundName} (found: {m_Background != null}), Thumb: {thumbName} (found: {m_Thumb != null})",
                    this);
                return;
            }

            // Pointerイベントの登録（背景エリアをタッチした時から操作を開始する）
            m_Background.RegisterCallback<PointerDownEvent>(OnPointerDown);
            m_Background.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            m_Background.RegisterCallback<PointerUpEvent>(OnPointerUp);
            m_Background.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        private void OnDestroy()
        {
            if (m_Background != null)
            {
                m_Background.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                m_Background.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                m_Background.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                m_Background.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            }

            ResetStick();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (m_IsDragging) return;

            m_IsDragging = true;
            m_StartPointerPosition = evt.position;

            // ポインターキャプチャを有効にし、ドラッグ中に指が背景範囲から出ても追従可能にする
            m_Background.CapturePointer(evt.pointerId);

            UpdateStickPosition(evt.position);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!m_IsDragging) return;
            if (!m_Background.HasPointerCapture(evt.pointerId)) return;

            UpdateStickPosition(evt.position);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!m_IsDragging) return;

            m_Background.ReleasePointer(evt.pointerId);
            m_IsDragging = false;

            ResetStick();
            evt.StopPropagation();
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            // ポインターキャプチャが何らかの理由で失われて離脱した場合はリセットする
            if (m_IsDragging && !m_Background.HasPointerCapture(evt.pointerId))
            {
                m_IsDragging = false;
                ResetStick();
            }
        }

        private void UpdateStickPosition(Vector2 pointerPosition)
        {
            // 開始位置からのドラッグ距離（スクリーン座標系でのオフセット）を計算
            Vector2 offset = pointerPosition - m_StartPointerPosition;

            // UI ToolkitのY軸は「下方向が正」
            // Input System（Vector2 / Gamepadのスティック）のY軸は「上方向が正」
            // よって入力値として送信する際にはY軸を反転させる
            Vector2 inputOffset = new Vector2(offset.x, -offset.y);

            // 最大半径 (movementRange) でクランプする
            float distance = inputOffset.magnitude;
            if (distance > movementRange)
            {
                inputOffset = inputOffset.normalized * movementRange;
                // UI表示（Translate）用のオフセットもクランプ値に連動させる
                offset = new Vector2(inputOffset.x, -inputOffset.y);
            }

            // つまみの表示位置を更新（UI ToolkitのTranslateスタイルを使用）
            m_Thumb.style.translate = new Translate(offset.x, offset.y, 0);

            // Input Systemのコントロール値として送信 (-1.0 ~ 1.0 に正規化)
            Vector2 normalizedInput = inputOffset / movementRange;
            SendValueToControl(normalizedInput);
        }

        private void ResetStick()
        {
            if (m_Thumb != null)
            {
                m_Thumb.style.translate = new Translate(0, 0, 0);
            }

            SendValueToControl(Vector2.zero);
        }
    }
}