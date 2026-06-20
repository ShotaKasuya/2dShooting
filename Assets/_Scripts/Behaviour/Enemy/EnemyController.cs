using _Scripts.Interface;
using _Scripts.Service;
using _Scripts.UnityServiceLocator;
using Module.EditorExtension.Runtime.Attribute;
using UnityEngine;

namespace _Scripts.Behaviour
{
    [RequireComponent(typeof(EnemyAI))]
    public class EnemyController : MonoBehaviour, IDamage, IEnemyEntity
    {
        private const string k_DeathEffectPoolKey = "EnemyDeathEffect";

        [SerializeField] private float maxHealth = 10f;
        [SerializeField, NullAssert] private TimedEffect deathEffectPrefab;
        [SerializeField] private float offScreenThreshold = 2f;

        private EnemyAI m_EnemyAI;
        private float m_CurrentHealth;
        private ObjectPoolService m_PoolService;
        private EnvironmentService m_EnvironmentService;
        private bool m_IsDead;
        private bool m_HasEnteredScreen;
        private Transform m_Transform;
        private string m_PoolKey;

        private void Awake()
        {
            m_EnemyAI = GetComponent<EnemyAI>();
            m_Transform = transform;
            m_CurrentHealth = maxHealth;
        }

        private void Start()
        {
            m_EnvironmentService = ServiceLocator.ForSceneOf(this).Get<EnvironmentService>();
            m_PoolService = ServiceLocator.ForSceneOf(this).Get<ObjectPoolService>();
            m_PoolService.CreatePool(k_DeathEffectPoolKey, deathEffectPrefab);
        }

        public void Setup(string poolKey = null)
        {
            m_PoolKey = poolKey;
            m_CurrentHealth = maxHealth;
            m_IsDead = false;
            m_HasEnteredScreen = false;
        }

        public bool IsRemaining => !m_IsDead;

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void DisActivate()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            CheckOffScreen();
        }

        private void CheckOffScreen()
        {
            if (m_EnvironmentService == null || m_IsDead) return;

            Camera cam = m_EnvironmentService.MainCamera;
            var pos = m_Transform.position;
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;
            var camPos = cam.transform.position;

            bool isInside = Mathf.Abs(pos.x - camPos.x) < camWidth && Mathf.Abs(pos.y - camPos.y) < camHeight;

            if (!m_HasEnteredScreen)
            {
                if (isInside)
                {
                    m_HasEnteredScreen = true;
                }

                return;
            }

            float limitX = camWidth + offScreenThreshold;
            float limitY = camHeight + offScreenThreshold;

            if (Mathf.Abs(pos.x - camPos.x) > limitX || Mathf.Abs(pos.y - camPos.y) > limitY)
            {
                HandleDeath();
            }
        }

        public void ApplyDamage(DamageData data)
        {
            if (m_IsDead) return;

            m_CurrentHealth -= data.Amount;

            m_EnemyAI.TakeKnock(data);

            if (m_CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            var effect = m_PoolService.Get<TimedEffect>(k_DeathEffectPoolKey);
            effect.Initialize(transform.position, Quaternion.identity, k_DeathEffectPoolKey);

            HandleDeath();
        }

        private void HandleDeath()
        {
            m_IsDead = true;
            if (string.IsNullOrEmpty(m_PoolKey))
            {
                Destroy(gameObject);
            }
            else
            {
                m_PoolService.Return(m_PoolKey, this);
            }
        }
    }
}