using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UIElements;

namespace _Scripts.Behaviour
{
    [RequireComponent(typeof(UIDocument))]
    public class UiTkOnScreenButton : OnScreenControl
    {
        [InputControl(layout = "Button")] [SerializeField]
        private string m_ControlPath = "<Keyboard>/enter";

        [Header("UI Element Names")] [SerializeField]
        private string buttonName = "enter-button";

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }

        private Button m_Button;

        private bool m_IsPressed;

        private void Awake()
        {
            var uiDocument = GetComponent<UIDocument>();

            if (uiDocument == null)
                return;

            var root = uiDocument.rootVisualElement;

            m_Button = root.Q<Button>(buttonName);

            if (m_Button == null)
            {
                Debug.LogError(
                    $"UiTkOnScreenButton: Button not found: {buttonName}",
                    this);

                return;
            }

            m_Button.RegisterCallback<PointerDownEvent>(OnPointerDown);
            m_Button.RegisterCallback<PointerUpEvent>(OnPointerUp);
            m_Button.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        private void OnDestroy()
        {
            if (m_Button != null)
            {
                m_Button.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                m_Button.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                m_Button.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            }

            ReleaseButton();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (m_IsPressed)
                return;

            m_IsPressed = true;

            m_Button.CapturePointer(evt.pointerId);

            SendValueToControl(1.0f);

            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!m_IsPressed)
                return;

            m_Button.ReleasePointer(evt.pointerId);

            ReleaseButton();

            evt.StopPropagation();
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            // キャプチャが失われた場合の保険
            if (m_IsPressed && !m_Button.HasPointerCapture(evt.pointerId))
            {
                ReleaseButton();
            }
        }

        private void ReleaseButton()
        {
            m_IsPressed = false;

            SendValueToControl(0.0f);
        }
    }
}