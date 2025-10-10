using UnityEngine;

namespace Core.Input
{
    /// <summary>
    /// Contract for abstracting Unity's Input System, providing type-safe input access
    /// </summary>
    public interface IInputService
    {
        /// <summary>
        /// Gets current value of a 2D vector input action (e.g., movement, look)
        /// </summary>
        /// <param name="actionName">Name of the input action</param>
        /// <returns>Current 2D input value</returns>
        public Vector2 GetVector2(string actionName);

        /// <summary>
        /// Checks if a button input action was pressed this frame
        /// </summary>
        /// <param name="actionName">Name of the input action</param>
        /// <returns>True if button was pressed this frame</returns>
        public bool GetButtonDown(string actionName);

        /// <summary>
        /// Checks if a button input action is currently held down
        /// </summary>
        /// <param name="actionName">Name of the input action</param>
        /// <returns>True if button is currently held</returns>
        public bool GetButton(string actionName);

        /// <summary>
        /// Checks if a button input action was released this frame
        /// </summary>
        /// <param name="actionName">Name of the input action</param>
        /// <returns>True if button was released this frame</returns>
        public bool GetButtonUp(string actionName);

        /// <summary>
        /// Enables all input processing
        /// </summary>
        public void EnableInput();

        /// <summary>
        /// Disables all input processing
        /// </summary>
        public void DisableInput();

        /// <summary>
        /// Checks if input processing is currently enabled
        /// </summary>
        /// <returns>True if input is enabled</returns>
        public bool IsInputEnabled();
    }
}
