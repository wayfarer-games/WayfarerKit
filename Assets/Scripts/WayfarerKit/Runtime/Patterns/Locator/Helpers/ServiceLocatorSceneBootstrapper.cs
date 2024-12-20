using UnityEngine;

namespace WayfarerKit.Patterns.Locator.Helpers
{
    [DisallowMultipleComponent, DefaultExecutionOrder(-1000), AddComponentMenu("Wayfarer/Settings/ServiceLocator Scene")]
    public sealed class ServiceLocatorSceneBootstrapper : LocatorBootstrapper
    {
        protected override void Bootstrap() => Container.ConfigureForScene();
    }
}