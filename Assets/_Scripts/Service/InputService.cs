using _Scripts.UnityServiceLocator;
using UnityEngine;

namespace _Scripts.Service
{
    public class InputService : GlobalService<InputService>
    {
        private InputSystem_Actions m_InputSystemActions;

        private InputSystem_Actions.PlayerActions m_PlayerActions;

        private void Start()
        {
            m_InputSystemActions = new InputSystem_Actions();

            m_PlayerActions = m_InputSystemActions.Player;
            m_InputSystemActions.Enable();
        }

        public Vector2 ReadMove()
        {
            var value = m_PlayerActions.Move.ReadValue<Vector2>();
            return value;
        }

        public Vector2 ReadLook() => m_PlayerActions.Look.ReadValue<Vector2>();
        public bool ReadFire() => m_PlayerActions.Attack.IsPressed();

        private void OnDestroy()
        {
            m_InputSystemActions?.Disable();
            m_InputSystemActions?.Dispose();
            m_InputSystemActions = null;
        }
    }
}