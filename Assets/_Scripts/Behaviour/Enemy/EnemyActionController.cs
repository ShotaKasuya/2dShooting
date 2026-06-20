using System;
using System.Collections.Generic;
using _Scripts.Model;
using _Scripts.Service;
using _Scripts.UnityServiceLocator;
using Module.EditorExtension.Runtime.Attribute;
using UnityEngine;

namespace _Scripts.Behaviour
{
    /// <summary>
    /// 敵の攻撃処理を制御するクラス。
    /// 通常状態と攻撃状態を交互に繰り返す。
    /// </summary>
    public class EnemyActionController : MonoBehaviour
    {
        public enum ActionState
        {
            k_Normal,
            k_Attack,
        }

        [Serializable]
        public struct StateConfig
        {
            public ActionState state;
            public float duration;
        }

        [SerializeField] private List<StateConfig> stateSequence;
        [SerializeField] private float rotationSpeed = 360f;
        [SerializeField] private float fireRate = 1f;
        [SerializeField, NullAssert] private Bullet bulletPrefab;
        [SerializeField] private AudioClipAsset shootSound;
        [SerializeField, NullAssert] private Transform firePoint;

        private int m_CurrentStateIndex;
        private float m_StateStartTimeStamp;
        private float m_FireTimer;

        private Transform m_Transform;
        private PlayerController m_Player;
        private ObjectPoolService m_PoolService;
        private AudioManager m_AudioManager;
        private EnemyAI m_EnemyAI;

        private const string k_EnemyBulletPoolKey = "EnemyBullet";

        private void Start()
        {
            m_Transform = transform;

            m_Player = ServiceLocator.ForSceneOf(this).Get<PlayerController>();
            m_PoolService = ServiceLocator.ForSceneOf(this).Get<ObjectPoolService>();
            m_EnemyAI = GetComponent<EnemyAI>();

            ServiceLocator.Global.TryGet(out m_AudioManager);
            Debug.Assert(m_AudioManager != null, "AudioManager not found in Global ServiceLocator.", this);
            Debug.Assert(shootSound != null, "shootSound is not assigned.", this);

            if (m_PoolService != null && bulletPrefab != null)
            {
                m_PoolService.CreatePool(k_EnemyBulletPoolKey, bulletPrefab);
            }

            // デフォルトのシーケンス設定
            if (stateSequence == null || stateSequence.Count == 0)
            {
                stateSequence = new List<StateConfig>
                {
                    new StateConfig { state = ActionState.k_Normal, duration = 3f },
                    new StateConfig { state = ActionState.k_Attack, duration = 3f }
                };
            }

            // EnemyAIの自動回転をオフにして、このクラスで制御する
            if (m_EnemyAI != null)
            {
                m_EnemyAI.SetAutoRotate(false);
            }
        }

        private void Update()
        {
            if (stateSequence == null || stateSequence.Count == 0) return;

            UpdateState();
            ExecuteState();
        }

        private void UpdateState()
        {
            if (Time.time - m_StateStartTimeStamp >= stateSequence[m_CurrentStateIndex].duration)
            {
                m_StateStartTimeStamp = Time.time;
                m_CurrentStateIndex = (m_CurrentStateIndex + 1) % stateSequence.Count;

                // 状態遷移時に射撃タイマーをリセット
                m_FireTimer = 0;
            }
        }

        private void ExecuteState()
        {
            var currentConfig = stateSequence[m_CurrentStateIndex];
            switch (currentConfig.state)
            {
                case ActionState.k_Normal:
                    ExecuteNormal();
                    break;
                case ActionState.k_Attack:
                    ExecuteAttack();
                    break;
            }
        }

        private void ExecuteNormal()
        {
            // 通常状態: 移動方向を向く
            if (m_EnemyAI != null)
            {
                RotateTowards(m_EnemyAI.CurrentMoveDirection);
            }
        }

        private void ExecuteAttack()
        {
            if (m_Player == null) return;

            // 攻撃状態: 一定速度でプレイヤーの方向を向く
            Vector2 toPlayer = (Vector2)m_Player.transform.position - (Vector2)m_Transform.position;
            RotateTowards(toPlayer);

            // 一定時間毎に攻撃を放つ
            m_FireTimer += Time.deltaTime;
            if (m_FireTimer >= fireRate)
            {
                m_FireTimer = 0;
                Shoot();
            }
        }

        private void RotateTowards(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.001f) return;

            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // 右正面のスプライトを想定（上が正面なら-90）
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle - 90f);
            m_Transform.rotation =
                Quaternion.RotateTowards(m_Transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void Shoot()
        {
            m_AudioManager.PlaySe(shootSound);

            var bullet = m_PoolService.Get<Bullet>(k_EnemyBulletPoolKey);
            bullet.SetUpBullet(firePoint.position, firePoint.rotation, "Player", k_EnemyBulletPoolKey);
        }
    }
}