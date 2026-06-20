using System.Collections.Generic;
using _Scripts.Interface;
using _Scripts.Model;
using _Scripts.Service;
using _Scripts.UnityServiceLocator;
using Module.EditorExtension.Runtime.Attribute;
using UnityEngine;

namespace _Scripts.Behaviour
{
    public class EnemyGate : MonoBehaviour, IDamage, IEnemyEntity
    {
        [SerializeField, NullAssert] private EnemyController enemyPrefab;
        [SerializeField] private AudioClipAsset spawnSound;
        [SerializeField] private float spawnInterval = 3f;
        [SerializeField] private float maxHealth = 20f;
        [SerializeField] private string poolKey = "EnemyGateSpawned";

        private float m_CurrentHealth;
        private float m_SpawnTimer;
        private ObjectPoolService m_PoolService;
        private AudioManager m_AudioManager;
        private readonly List<EnemyController> m_SpawnedEnemies = new();
        private bool m_IsDead;

        private void Start()
        {
            m_CurrentHealth = maxHealth;
            m_PoolService = ServiceLocator.ForSceneOf(this).Get<ObjectPoolService>();
            m_PoolService.CreatePool(poolKey, enemyPrefab);

            ServiceLocator.Global.TryGet(out m_AudioManager);
            Debug.Assert(m_AudioManager != null, "AudioManager not found in Global ServiceLocator.", this);
            Debug.Assert(spawnSound != null, "spawnSound is not assigned.", this);
        }

        public bool IsRemaining
        {
            get
            {
                // Gate自体が生きている（死亡していない）、もしくは生成した敵がまだ残っている
                m_SpawnedEnemies.RemoveAll(x => x == null || !x.gameObject.activeInHierarchy);
                return !m_IsDead || m_SpawnedEnemies.Count > 0;
            }
        }

        public void Activate()
        {
            m_IsDead = false;
            gameObject.SetActive(true);
        }

        public void DisActivate()
        {
            gameObject.SetActive(false);
            foreach (var enemy in m_SpawnedEnemies)
            {
                if (enemy != null) enemy.DisActivate();
            }
        }

        private void Update()
        {
            if (m_IsDead) return;

            m_SpawnTimer += Time.deltaTime;
            if (m_SpawnTimer >= spawnInterval)
            {
                m_SpawnTimer = 0;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            var enemy = m_PoolService.Get<EnemyController>(poolKey);
            if (enemy != null)
            {
                m_AudioManager.PlaySe(spawnSound);

                enemy.transform.position = transform.position;
                enemy.transform.rotation = Quaternion.identity;
                enemy.Setup(poolKey);
                enemy.Activate();
                m_SpawnedEnemies.Add(enemy);
            }
        }

        public void ApplyDamage(DamageData data)
        {
            m_CurrentHealth -= data.Amount;
            if (m_CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            m_IsDead = true;
            gameObject.SetActive(false);
        }
    }
}