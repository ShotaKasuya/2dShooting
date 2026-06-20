using _Scripts.Service;
using _Scripts.UnityServiceLocator;
using UnityEngine;

namespace _Scripts.Behaviour
{
    public class EnemyAI : MonoBehaviour
    {
        public enum AIState
        {
            k_MoveToCoordinate,
            k_MoveOnCurve,
            k_PatrolPoints,
        }

        [SerializeField] private AIState currentState = AIState.k_MoveToCoordinate;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 360f;
        [SerializeField] private bool autoRotate = true;

        public Vector2 CurrentMoveDirection { get; private set; }

        private float m_CurveTimer;
        private int m_CurrentWaypointIndex;

        private Transform m_Transform;
        private EnemyEnvironmentService m_EnemyEnvironmentService;

        private void Start()
        {
            m_Transform = transform;
            
            // サービスロケータから環境情報を取得
            m_EnemyEnvironmentService = ServiceLocator.ForSceneOf(this).Get<EnemyEnvironmentService>();
        }

        private void Update()
        {
            switch (currentState)
            {
                case AIState.k_MoveToCoordinate:
                    ExecuteMoveToCoordinate();
                    break;
                case AIState.k_MoveOnCurve:
                    ExecuteMoveOnCurve();
                    break;
                case AIState.k_PatrolPoints:
                    ExecutePatrol();
                    break;
            }
        }

        private void ExecuteMoveToCoordinate()
        {
            if (m_EnemyEnvironmentService == null) return;

            Vector2 currentPos = m_Transform.position;
            Vector2 target = m_EnemyEnvironmentService.TargetCoordinate;
            Vector2 moveDir = target - currentPos;
            CurrentMoveDirection = moveDir;

            m_Transform.position = Vector2.MoveTowards(currentPos, target, moveSpeed * Time.deltaTime);

            RotateTowards(moveDir);

            if (Vector2.Distance(m_Transform.position, target) < m_EnemyEnvironmentService.ArrivalDistance)
            {
                // 到着後の挙動（例：次の状態へ）などはインスペクターで切り替え可能
            }
        }

        private void ExecuteMoveOnCurve()
        {
            if (m_EnemyEnvironmentService == null) return;

            m_CurveTimer += Time.deltaTime * m_EnemyEnvironmentService.Frequency;

            // リサージュ曲線（8の字のような動き）
            float x = m_EnemyEnvironmentService.CurvePivot.x + Mathf.Sin(m_CurveTimer) * m_EnemyEnvironmentService.AmplitudeX;
            float y = m_EnemyEnvironmentService.CurvePivot.y + Mathf.Sin(m_CurveTimer * 2f) * m_EnemyEnvironmentService.AmplitudeY;

            Vector2 nextPos = new Vector2(x, y);
            Vector2 moveDir = nextPos - (Vector2)m_Transform.position;
            CurrentMoveDirection = moveDir;
            
            m_Transform.position = nextPos;
            RotateTowards(moveDir);
        }

        private void ExecutePatrol()
        {
            if (m_EnemyEnvironmentService == null) return;
            var waypoints = m_EnemyEnvironmentService.PatrolWaypoints;
            if (waypoints == null || waypoints.Count == 0) return;

            Transform targetTransform = waypoints[m_CurrentWaypointIndex];
            if (targetTransform == null)
            {
                // Transformが未設定の場合はスキップ
                m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Count;
                return;
            }

            Vector2 target = targetTransform.position;
            Vector2 currentPos = m_Transform.position;
            Vector2 moveDir = target - currentPos;
            CurrentMoveDirection = moveDir;
            
            m_Transform.position = Vector2.MoveTowards(currentPos, target, moveSpeed * Time.deltaTime);
            RotateTowards(moveDir);

            if (Vector2.Distance(m_Transform.position, target) < m_EnemyEnvironmentService.ArrivalDistance)
            {
                m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Count;
            }
        }

        private void RotateTowards(Vector2 direction)
        {
            if (!autoRotate || direction.sqrMagnitude < 0.001f) return;

            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // プレイヤー同様のスプライト向きを想定（右が正面ならオフセットなし、上が正面なら-90など）
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle - 90f);
            m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        public void TakeKnock(in DamageData damageData)
        {
            // 簡易的なノックバック
            Vector2 knockbackDir = (Vector2)m_Transform.position - damageData.HitPosition;
            m_Transform.Translate(knockbackDir.normalized * damageData.KnockbackForce, Space.World);
        }

        // 状態を外部から切り替えるためのメソッド
        public void SetState(AIState newState) => currentState = newState;
        public void SetAutoRotate(bool value) => autoRotate = value;
    }
}