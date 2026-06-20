using _Scripts.Model;
using _Scripts.Service;
using _Scripts.UnityServiceLocator;
using Module.EditorExtension.Runtime.Attribute;
using UnityEngine;

namespace _Scripts.Behaviour
{
    public class PlayerShooting : MonoBehaviour
    {
        private const string k_PlayerBulletPrefab = "PlayerBullet";
        [SerializeField, NullAssert] private Bullet bulletPrefab;
        [SerializeField] private AudioClipAsset shootSound;
        [SerializeField, NullAssert] private Transform firePoint;
        [SerializeField] private float fireRate = 0.2f;

        [SerializeField] private bool isActivated = true;
        public bool IsActivated { get => isActivated; set => isActivated = value; }

        private InputService m_InputService;
        private ObjectPoolService m_PoolService;
        private AudioManager m_AudioManager;
        private float m_NextFireTime;

        private void Start()
        {
            m_InputService = ServiceLocator.Global.Get<InputService>();
            m_PoolService = ServiceLocator.ForSceneOf(this).Get<ObjectPoolService>();

            m_PoolService.CreatePool(k_PlayerBulletPrefab, bulletPrefab);

            ServiceLocator.Global.TryGet(out m_AudioManager);
            Debug.Assert(m_AudioManager != null, "AudioManager not found in Global ServiceLocator.", this);
            Debug.Assert(shootSound != null, "shootSound is not assigned.", this);
        }

        private void Update()
        {
            if (!isActivated) return;

            if (m_InputService.ReadFire() && Time.time >= m_NextFireTime)
            {
                Shoot();
                m_NextFireTime = Time.time + fireRate;
            }
        }

        private void Shoot()
        {
            m_AudioManager.PlaySe(shootSound);

            // プレイヤーの向き（firePoint の右方向）に弾を生成
            var bullet = m_PoolService.Get<Bullet>(k_PlayerBulletPrefab);
            bullet.SetUpBullet(firePoint.position, firePoint.rotation, "Enemy", k_PlayerBulletPrefab);
        }
    }
}