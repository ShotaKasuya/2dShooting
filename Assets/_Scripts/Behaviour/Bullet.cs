using _Scripts.Model;
using _Scripts.Service;
using _Scripts.UnityServiceLocator;
using Module.EditorExtension.Runtime.Attribute;
using UnityEngine;

namespace _Scripts.Behaviour
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField, AutoAssign] private Transform selfTransform;
        [SerializeField, NullAssert] private TimedEffect finishEffect;
        [SerializeField] private AudioClipAsset hitSound;
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifeTime = 3f;

        private const string k_FinnishEffect = "FinnishEffect";
        private ObjectPoolService m_PoolService;
        private AudioManager m_AudioManager;
        private string m_TargetTag;
        private string m_PoolKey;
        private float m_BulletDeadTime;

        private void Start()
        {
            m_PoolService = ServiceLocator.ForSceneOf(this).Get<ObjectPoolService>();
            m_PoolService.CreatePool(k_FinnishEffect, finishEffect);

            ServiceLocator.Global.TryGet(out m_AudioManager);
            Debug.Assert(m_AudioManager != null, "AudioManager not found in Global ServiceLocator.", this);
            Debug.Assert(hitSound != null, "hitSound is not assigned.", this);
        }

        public void SetUpBullet(Vector3 position, Quaternion rotation, string targetTag, string poolKey)
        {
            selfTransform.position = position;
            selfTransform.rotation = rotation;
            m_TargetTag = targetTag;
            m_PoolKey = poolKey;

            m_BulletDeadTime = Time.time + lifeTime;
        }

        private void Update()
        {
            selfTransform.Translate(Vector3.up * (speed * Time.deltaTime));

            if (m_BulletDeadTime < Time.time)
            {
                Finish();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(m_TargetTag) && other.TryGetComponent<IDamage>(out var damage))
            {
                damage.ApplyDamage(new DamageData(1, 1, selfTransform.position));

                m_AudioManager.PlaySe(hitSound);

                Finish();
            }
        }

        private void Finish()
        {
            // プールに戻す
            m_PoolService.Return(m_PoolKey, this);
            var effect = m_PoolService.Get<TimedEffect>(k_FinnishEffect);
            effect.Initialize(selfTransform.position, selfTransform.rotation, k_FinnishEffect);
        }
    }
}