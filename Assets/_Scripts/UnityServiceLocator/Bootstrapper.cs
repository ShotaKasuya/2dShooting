using UnityEngine;

namespace _Scripts.UnityServiceLocator
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ServiceLocator))]
    public abstract class Bootstrapper : MonoBehaviour
    {
        private ServiceLocator m_Container;
        internal ServiceLocator Container => m_Container.OrNull() ?? (m_Container = GetComponent<ServiceLocator>());

        private bool m_HasBeenBootstrapped;

        private void Awake() => BootstrapOnDemand();

        public void BootstrapOnDemand()
        {
            if (m_HasBeenBootstrapped)
            {
                return;
            }

            m_HasBeenBootstrapped = true;
            Bootstrap();
        }

        protected abstract void Bootstrap();
    }
}