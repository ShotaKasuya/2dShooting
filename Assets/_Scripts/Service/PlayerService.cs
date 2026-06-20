using _Scripts.UnityServiceLocator;
using UnityEngine;

namespace _Scripts.Service
{
    public class PlayerService:MonoBehaviour
    {
        private Transform m_Transform;

        public Transform Transform => m_Transform;

        private void Awake()
        {
            m_Transform = transform;
            
            ServiceLocator.ForSceneOf(this).Register(this);
        }
    }
}