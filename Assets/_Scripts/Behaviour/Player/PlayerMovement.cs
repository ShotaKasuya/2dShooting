using _Scripts.Service;
using _Scripts.UnityServiceLocator;
using Module.EditorExtension.Runtime.VisualDebugger;
using UnityEngine;

namespace _Scripts.Behaviour
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private bool isActivated = true;
        public bool IsActivated { get => isActivated; set => isActivated = value; }

        [SerializeField] private float speed = 2f;
        [SerializeField] private float accel = 4f;
        [SerializeField] private float rotationSpeed = 720f;
        [SerializeField] private float rotationOffset = -90f;

        [Header("Knockback Settings")] [SerializeField]
        private float knockbackDecay = 5f;

        [SerializeField] private float knockbackResistance = 3f;

        [Header("Boundary Settings")] [SerializeField]
        private float padding = 0.5f;

        private Vector2 m_CurrentSpeed;
        private Vector2 m_KnockbackVelocity;
        private Transform m_Transform;
        private EnvironmentService m_EnvironmentService;

        private InputService m_InputService;

        private Vector2 m_Integral;
        private Vector2 m_PreviousError;

        private void Start()
        {
            m_Transform = transform;
            m_InputService = ServiceLocator.Global.Get<InputService>();
            m_EnvironmentService = ServiceLocator.ForSceneOf(this).Get<EnvironmentService>();
        }

        private void Update()
        {
            if (!isActivated) return;

            var moveInput = m_InputService.ReadMove();
            var lookInput = m_InputService.ReadLook();

            var deltaTime = Time.deltaTime;
            var targetSpeed = CalcInputMovement(moveInput, deltaTime);

            // ノックバックの減衰率の計算
            float currentDecay = knockbackDecay;
            if (m_KnockbackVelocity.sqrMagnitude > 0.001f && moveInput.sqrMagnitude > 0.001f)
            {
                float dot = Vector2.Dot(moveInput.normalized, m_KnockbackVelocity.normalized);
                currentDecay -= dot * knockbackResistance;
                currentDecay = Mathf.Max(0.1f, currentDecay);
            }

            // ノックバック速度の減衰
            m_KnockbackVelocity = Vector2.MoveTowards(m_KnockbackVelocity, Vector2.zero, currentDecay * deltaTime);

            // 移動実行
            m_Transform.Translate((targetSpeed + m_KnockbackVelocity) * deltaTime, Space.World);

            // 回転処理
            Rotate(moveInput, lookInput, deltaTime);

            // カメラ内に制限
            RestrictToCameraBounds();
        }

        private void Rotate(Vector2 moveInput, Vector2 lookInput, float deltaTime)
        {
            Vector2 targetDirection = Vector2.zero;

            // Look入力があれば優先、なければ移動方向
            if (lookInput.sqrMagnitude > 0.001f)
            {
                targetDirection = lookInput;
            }
            else if (moveInput.sqrMagnitude > 0.001f)
            {
                targetDirection = moveInput;
            }

            if (targetDirection != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle + rotationOffset);
                m_Transform.rotation =
                    Quaternion.RotateTowards(m_Transform.rotation, targetRotation, rotationSpeed * deltaTime);
            }
        }

        private void RestrictToCameraBounds()
        {
            if (m_EnvironmentService == null) return;

            Camera cam = m_EnvironmentService.MainCamera;
            if (cam == null) return;

            var pos = m_Transform.position;

            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            var camPos = cam.transform.position;
            float minX = camPos.x - camWidth + padding;
            float maxX = camPos.x + camWidth - padding;
            float minY = camPos.y - camHeight + padding;
            float maxY = camPos.y + camHeight - padding;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            m_Transform.position = pos;
        }

        public void ApplyDamage(DamageData data)
        {
            var knockBackVector = m_Transform.position.As2() - data.HitPosition;
            m_KnockbackVelocity = knockBackVector.normalized * data.KnockbackForce;
        }

        private Vector2 CalcInputMovement(Vector2 input, float deltaTime)
        {
            m_CurrentSpeed = Vector2.MoveTowards(m_CurrentSpeed, input * speed, accel * deltaTime);

            return m_CurrentSpeed;
        }
    }
}