using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.Assertions;
using WayfarerKit.Patterns.Singletons;
using WayfarerKit.Patterns.Singletons.Helpers;

namespace WayfarerKit.Systems.SceneManagement
{
    [CreateAssetMenu(fileName = "BootstrapperSettings", menuName = "WayfarerSDK/Settings/BootstrapperSettings", order = 1), ScriptableObjectPath("Settings/BootstrapperSettings")]
    public sealed class BootstrapperSettings : ScriptableObjectSingleton<BootstrapperSettings>
    {
        [SerializeField] private SceneReference bootstrapperScene;
        [SerializeField] private GameObject bootstrapperPrefab;
        [SerializeField] private bool reloadDuplicateScenes;

        [Header("Debug settings"), Tooltip("If true, the bootstrapper will always start with the bootstrapper scene. If false, it will start with the current opened scene in Unity editor."), SerializeField]
        private bool alwaysStartWithBootstrapperScene;

        [SerializeField] private float sceneSwitchDelay = 0.5f;

        public bool ReloadDuplicateScenes => reloadDuplicateScenes;
        public string BootstrapperSceneName => bootstrapperScene.Name;
        public SceneReference BootstrapperSceneReference => bootstrapperScene;
        public GameObject BootstrapperPrefab => bootstrapperPrefab;

        public bool AlwaysStartWithBootstrapperScene => alwaysStartWithBootstrapperScene;
        public float SceneSwitchDelay => sceneSwitchDelay;

        private void Awake()
        {
            Assert.IsNotNull(bootstrapperScene, "Please set the bootstrapper scene in BootstrapperSettings.");
            Assert.IsNotNull(bootstrapperPrefab, "Please set the bootstrapper prefab in BootstrapperSettings.");
        }
    }
}