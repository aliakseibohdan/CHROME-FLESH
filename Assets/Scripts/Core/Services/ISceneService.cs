using System.Collections;

namespace Core.Services
{
    /// <summary>
    /// Manages scene loading, unloading, and scene lifecycle transitions
    /// </summary>
    public interface ISceneService
    {
        /// <summary>
        /// Asynchronously loads a scene by name
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        /// <returns>Coroutine enumerator for async operation</returns>
        public IEnumerator LoadSceneAsync(string sceneName);

        /// <summary>
        /// Asynchronously loads a scene additive to current scenes
        /// </summary>
        /// <param name="sceneName">Name of the scene to load additively</param>
        /// <returns>Coroutine enumerator for async operation</returns>
        public IEnumerator LoadSceneAdditiveAsync(string sceneName);

        /// <summary>
        /// Asynchronously unloads a scene by name
        /// </summary>
        /// <param name="sceneName">Name of the scene to unload</param>
        /// <returns>Coroutine enumerator for async operation</returns>
        public IEnumerator UnloadSceneAsync(string sceneName);

        /// <summary>
        /// Gets the name of the currently active scene
        /// </summary>
        /// <returns>Name of active scene</returns>
        public string GetActiveSceneName();
    }
}
