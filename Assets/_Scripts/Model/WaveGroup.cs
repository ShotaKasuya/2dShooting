using System.Collections.Generic;
using _Scripts.Interface;
using UnityEngine;
using ZLinq;

namespace _Scripts.Model
{
    /// <summary>
    /// 1つのWaveが持つ敵すべてを管理する
    /// </summary>
    public class WaveGroup : MonoBehaviour
    {
        private List<IEnemyEntity> m_EnemyControllerList;

        public void Setup()
        {
            m_EnemyControllerList = new List<IEnemyEntity>(GetComponentsInChildren<IEnemyEntity>(true));

            foreach (var controller in m_EnemyControllerList)
            {
                if (controller is MonoBehaviour mb && mb)
                {
                    controller.DisActivate();
                }
            }
        }

        public bool IsWaveActive()
        {
            return m_EnemyControllerList.AsValueEnumerable()
                .Any(x => x is MonoBehaviour mb && mb && x.IsRemaining);
        }

        public void WaveStart()
        {
            gameObject.SetActive(true);
            foreach (var controller in m_EnemyControllerList)
            {
                controller.Activate();
            }
        }

        public void WaveEnd()
        {
            gameObject.SetActive(false);
            foreach (var controller in m_EnemyControllerList)
            {
                controller.DisActivate();
            }
        }
    }
}