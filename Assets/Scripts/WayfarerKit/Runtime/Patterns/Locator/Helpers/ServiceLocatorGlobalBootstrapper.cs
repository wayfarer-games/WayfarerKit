using UnityEngine;

namespace WayfarerKit.Patterns.Locator.Helpers
{
    [DisallowMultipleComponent, DefaultExecutionOrder(-1000), AddComponentMenu("Wayfarer/Settings/ServiceLocator Global")]
    public sealed class ServiceLocatorGlobalBootstrapper : LocatorBootstrapper
    {
        [SerializeField] private bool dontDestroyOnLoad = true;

        protected override void Bootstrap() => Container.ConfigureAsGlobal(dontDestroyOnLoad);
    }
}