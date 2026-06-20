using _Scripts.UnityServiceLocator;
using UnityEngine;

namespace _Scripts.Service
{
    /// <summary>
    /// カメラなどの環境情報を管理し、サービスロケータ経由で提供するクラス。
    /// </summary>
    public class EnvironmentService : SceneService<EnvironmentService>
    {
        private Camera m_MainCamera;

        private void Start()
        {
            m_MainCamera = Camera.main;
        }

        public Camera MainCamera => m_MainCamera;

        /// <summary>
        /// 指定された位置が画面内かどうかを判定する。
        /// </summary>
        public bool IsInScreen(Vector3 position, float threshold = 0f)
        {
            if (m_MainCamera == null) return true;

            Vector3 viewPos = m_MainCamera.WorldToViewportPoint(position);
            return viewPos.x >= -threshold && viewPos.x <= 1f + threshold && 
                   viewPos.y >= -threshold && viewPos.y <= 1f + threshold;
        }
    }
}
