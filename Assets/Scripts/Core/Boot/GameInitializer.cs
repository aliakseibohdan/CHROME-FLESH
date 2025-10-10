using UnityEngine;
using System.Collections;
using Core.Services;
using Core.Events;
using Core.Input;

namespace Core.Boot
{
    /// <summary>
    /// Main entry point for the game initialization sequence.
    /// Attach this component to a GameObject in the Bootstrap scene.
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] private string _firstSceneName = "MainMenu";
        [SerializeField] private bool _verboseLogging = true;

        private IServiceContainer _serviceContainer;

        private void Awake()
        {
            if (_verboseLogging)
            {
                Debug.Log("[GameInitializer] Starting boot sequence...");
            }

            InitializeCoreServices();
            _ = StartCoroutine(LoadFirstSceneAsync());
        }

        private void InitializeCoreServices()
        {
            _serviceContainer = new ServiceContainer();

            ServiceLocator.Initialize(_serviceContainer);

            _serviceContainer.Register<ISceneService>(new SceneService());
            _serviceContainer.Register<IEventBus>(new EventBus());
            _serviceContainer.Register<IInputService>(new InputService());

            if (_verboseLogging)
            {
                Debug.Log("[GameInitializer] Core services initialized and registered");
                Debug.Log($"[GameInitializer] ServiceLocator ready: {ServiceLocator.IsRegistered<ISceneService>()}");
            }
        }

        private IEnumerator LoadFirstSceneAsync()
        {
            if (_verboseLogging)
            {
                Debug.Log($"[GameInitializer] Loading initial scene: {_firstSceneName}");
            }

            var sceneService = ServiceLocator.Resolve<ISceneService>();
            yield return StartCoroutine(sceneService.LoadSceneAsync(_firstSceneName));

            if (_verboseLogging)
            {
                Debug.Log("[GameInitializer] Boot sequence completed");
            }
        }
    }
}
