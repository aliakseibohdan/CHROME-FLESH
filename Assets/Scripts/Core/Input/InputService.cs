using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Input
{
    /// <summary>
    /// Implementation of IInputService using Unity's new Input System
    /// </summary>
    public class InputService : IInputService
    {
        private PlayerInput _playerInput;
        private InputActionAsset _inputActions;
        private bool _isEnabled = true;

        public InputService() => InitializeInputSystem();

        private void InitializeInputSystem()
        {
            GameObject inputObject = new("InputService_PlayerInput");
            Object.DontDestroyOnLoad(inputObject);

            _playerInput = inputObject.AddComponent<PlayerInput>();

            _inputActions = Resources.Load<InputActionAsset>("Input/PlayerControls");
            _playerInput.actions = _inputActions;

            Debug.Log("[InputService] Input system initialized");
        }

        /// <summary>
        /// Gets current value of a 2D vector input action
        /// </summary>
        public Vector2 GetVector2(string actionName)
        {
            if (!_isEnabled)
            {
                return Vector2.zero;
            }

            var action = _playerInput.actions[actionName];
            return action?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        /// <summary>
        /// Checks if a button input action was pressed this frame
        /// </summary>
        public bool GetButtonDown(string actionName)
        {
            if (!_isEnabled)
            {
                return false;
            }

            var action = _playerInput.actions[actionName];
            return action?.triggered ?? false;
        }

        /// <summary>
        /// Checks if a button input action is currently held down
        /// </summary>
        public bool GetButton(string actionName)
        {
            if (!_isEnabled)
            {
                return false;
            }

            var action = _playerInput.actions[actionName];
            return action?.IsPressed() ?? false;
        }

        /// <summary>
        /// Checks if a button input action was released this frame
        /// </summary>
        public bool GetButtonUp(string actionName)
        {
            if (!_isEnabled)
            {
                return false;
            }

            var action = _playerInput.actions[actionName];
            return action?.WasReleasedThisFrame() ?? false;
        }

        /// <summary>
        /// Enables all input processing
        /// </summary>
        public void EnableInput()
        {
            _isEnabled = true;
            _playerInput.ActivateInput();
            Debug.Log("[InputService] Input enabled");
        }

        /// <summary>
        /// Disables all input processing
        /// </summary>
        public void DisableInput()
        {
            _isEnabled = false;
            _playerInput.DeactivateInput();
            Debug.Log("[InputService] Input disabled");
        }

        /// <summary>
        /// Checks if input processing is currently enabled
        /// </summary>
        public bool IsInputEnabled() => _isEnabled;
    }
}
