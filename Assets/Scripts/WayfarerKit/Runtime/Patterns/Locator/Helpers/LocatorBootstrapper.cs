using UnityEngine;
using UnityEngine.Assertions;

namespace WayfarerKit.Patterns.Locator.Helpers
{
    [DisallowMultipleComponent, RequireComponent(typeof(ServiceLocator))]
    public abstract class LocatorBootstrapper : MonoBehaviour
    {
        private ServiceLocator _container;
        private bool _hasBeenBootstrapped;

        internal ServiceLocator Container
        {
            get
            {
                if (_container == null) _container = GetComponent<ServiceLocator>();

                Assert.IsNotNull(_container);

                return _container;
            }
        }

        private void Awake() => BootstrapOnDemand();

        public void BootstrapOnDemand()
        {
            if (_hasBeenBootstrapped) return;

            _hasBeenBootstrapped = true;

            Bootstrap();
        }

        protected abstract void Bootstrap();
    }
}