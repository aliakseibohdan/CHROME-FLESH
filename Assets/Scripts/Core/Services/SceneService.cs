using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Core.Services
{
    /// <summary>
    /// Implementation of ISceneService using Unity's SceneManager
    /// </summary>
    public class SceneService : ISceneService
    {
        private string _currentScene;
        private readonly MonoBehaviour _coroutineRunner;

        public SceneService()
        {
            GameObject go = new("SceneService_CoroutineRunner");
            Object.DontDestroyOnLoad(go);
            _coroutineRunner = go.AddComponent<SceneServiceCoroutineRunner>();
        }

        /// <summary>
        /// Asynchronously loads a scene by name
        /// </summary>
        public IEnumerator LoadSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneService] Scene name cannot be null or empty");
                yield break;
            }

            var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            asyncOp.allowSceneActivation = true;

            while (!asyncOp.isDone)
            {
                yield return null;
            }

            _currentScene = sceneName;
            Debug.Log($"[SceneService] Scene loaded: {sceneName}");
        }

        /// <summary>
        /// Asynchronously loads a scene additive to current scenes
        /// </summary>
        public IEnumerator LoadSceneAdditiveAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneService] Scene name cannot be null or empty");
                yield break;
            }

            var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOp.allowSceneActivation = true;

            while (!asyncOp.isDone)
            {
                yield return null;
            }

            Debug.Log($"[SceneService] Additive scene loaded: {sceneName}");
        }

        /// <summary>
        /// Asynchronously unloads a scene by name
        /// </summary>
        public IEnumerator UnloadSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneService] Scene name cannot be null or empty");
                yield break;
            }

            var asyncOp = SceneManager.UnloadSceneAsync(sceneName);

            if (asyncOp == null)
            {
                Debug.LogError($"[SceneService] Scene '{sceneName}' not found or cannot be unloaded");
                yield break;
            }

            while (!asyncOp.isDone)
            {
                yield return null;
            }

            Debug.Log($"[SceneService] Scene unloaded: {sceneName}");
        }

        /// <summary>
        /// Gets the name of the currently active scene
        /// </summary>
        public string GetActiveSceneName() => SceneManager.GetActiveScene().name;

        private class SceneServiceCoroutineRunner : MonoBehaviour { }
    }
}
