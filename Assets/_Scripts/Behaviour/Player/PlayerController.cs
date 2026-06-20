using _Scripts.UnityServiceLocator;
using UnityEngine;

namespace _Scripts.Behaviour
{
    /// <summary>
    /// プレイヤー全体の状態を管理するクラス。
    /// 移動や射撃などの各コンポーネントを制御する。
    /// </summary>
    public class PlayerController : SceneService<PlayerController>, IDamage
    {
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerShooting playerShooting;

        [SerializeField] private int maxHp;

        public Transform PlayerTransform => transform;
        public bool IsLiving => m_CurrentHp >= 0;

        private int m_CurrentHp;

        private void Start()
        {
            m_CurrentHp = maxHp;
        }

        /// <summary>
        /// プレイヤーの全機能を停止させる。
        /// </summary>
        public void StopPlayer()
        {
            if (playerMovement != null)
            {
                playerMovement.IsActivated = false;
            }

            if (playerShooting != null)
            {
                playerShooting.IsActivated = false;
            }
        }

        public void ApplyDamage(DamageData data)
        {
            m_CurrentHp -= data.Amount;
            playerMovement.ApplyDamage(data);
        }
    }
}
