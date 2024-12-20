using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WayfarerKit.Systems.Assets
{
    /// <summary>
    ///     Interface for asset loading system as part of Locator pattern.
    /// </summary>
    public interface IAssetProvider
    {
        public Awaitable<GameObject> Get(AssetReference reference, CancellationToken cancellationToken = default);
        public Awaitable<T> Get<T>(AssetReference reference, CancellationToken cancellationToken = default) where T : Object;

        public Awaitable<GameObject> Instantiate(AssetReference reference, Transform parent = null, CancellationToken cancellationToken = default);

        public void ReleaseFor(AssetReference reference);

        public void LogVerbatimState();
    }
}