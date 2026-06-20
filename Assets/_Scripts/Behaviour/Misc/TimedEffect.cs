using _Scripts.Service;
using _Scripts.UnityServiceLocator;
using Module.EditorExtension.Runtime.Attribute;
using UnityEngine;
using UnityEngine.VFX;

namespace _Scripts.Behaviour
{
    /// <summary>
    /// 一定時間経過後に自身を非アクティブ化、または破棄するコンポーネント。
    /// エフェクトやパーティクル、一時的な表示物に使用します。
    /// </summary>
    public class TimedEffect : MonoBehaviour
    {
        [SerializeField, AutoAssign] private Transform selfTransform;
        [SerializeField, AutoAssign] private VisualEffect visualEffect;
        [SerializeField] private float duration = 1.0f;

        private string m_PoolKey;
        private float m_Timer;
        private ObjectPoolService m_PoolService;

        private void Start()
        {
            m_PoolService = ServiceLocator.ForSceneOf(this).Get<ObjectPoolService>();
        }

        private void Update()
        {
            if (m_Timer <= Time.time)
            {
                Finish();
            }
        }

        private void Finish()
        {
            // プールに戻す
            m_PoolService.Return(m_PoolKey, this);
        }

        /// <summary>
        /// 外部から持続時間とプールキーを設定するAPI
        /// </summary>
        public void Initialize(Vector3 position, Quaternion rotation, string poolKey)
        {
            selfTransform.position = position;
            selfTransform.rotation = rotation;
            m_PoolKey = poolKey;
            m_Timer = Time.time + duration;

            visualEffect.Play();
        }
    }
}