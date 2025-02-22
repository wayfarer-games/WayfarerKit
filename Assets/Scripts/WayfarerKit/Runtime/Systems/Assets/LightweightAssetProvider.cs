using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace WayfarerKit.Systems.Assets
{
    [Serializable, Preserve]
    public class AddressablesRemoteConfig
    {
        public List<AddressablesRemoteEntry> Entries;
        [Serializable]
        public class AddressablesRemoteEntry
        {
            public string version;
            public string assetsURL;
            public string catalogName;
            public string bundleName;
        }
    }


    /// <summary>
    ///     Load assets using Addressables and cache them in memory.
    /// </summary>
    public class LightweightAssetProvider : IAssetProvider
    {
        private readonly Dictionary<string, AsyncOperationHandle<Object>> _cache = new();
        private AsyncOperationHandle<IResourceLocator> _addressablesInitHandle;

        private readonly AwaitableCompletionSource _assetProviderSource = new();
        private AsyncOperationHandle _downloadDepsHandle;
        private bool _isFirstLaunch;
        private bool _isNetworkReachable;
        private string[] _labelsToDownload;
        private AsyncOperationHandle<IResourceLocator> _loadCatalogHandle;
        private string _remoteCatalogPath;
        private string _remoteLoadDataJson;
        private Action<float> _reporter;
        private AsyncOperationHandle<List<IResourceLocator>> _updateCatalogHandle;

        public LightweightAssetProvider() => ResourceManager.ExceptionHandler += ExceptionHandler;

        public void SubscribeOnUpdateAssetsProgress(Action<float> reporter) => _reporter = reporter;

        public async Awaitable Init(string baseURL, string[] labelsToDownload)
        {
            _labelsToDownload = labelsToDownload;

            var remoteLoadData = await GetAssetBundleURL(baseURL);
            if (remoteLoadData == null)
            {
                Log.Warning($"init, failed to get url {baseURL}");
                return;
            }

            _isNetworkReachable = Application.internetReachability != NetworkReachability.NotReachable;
            _remoteLoadDataJson = JsonConvert.SerializeObject(remoteLoadData);

            _remoteCatalogPath = Path.Join(
                remoteLoadData.assetsURL,
                $"{remoteLoadData.catalogName}.bin"
            );

            // TODO: move to startup
            _addressablesInitHandle = Addressables.InitializeAsync();
            _addressablesInitHandle.Completed += OnAddressablesInitAction;

            while (!_assetProviderSource.Awaitable.IsCompleted)
            {
                var progress = 0f;
                if (_addressablesInitHandle.IsValid()) progress += 0.25f * _addressablesInitHandle.PercentComplete;

                if (_loadCatalogHandle.IsValid()) progress += 0.25f * (1 + _loadCatalogHandle.PercentComplete);

                if (_downloadDepsHandle.IsValid()) progress += 0.25f * (2 + _downloadDepsHandle.PercentComplete);

                if (_updateCatalogHandle.IsValid()) progress += 0.25f * (3 + _updateCatalogHandle.PercentComplete);

                _reporter?.Invoke(progress);
                await Awaitable.NextFrameAsync();
            }

            _reporter = null;
        }

        private void OnAddressablesInitAction(AsyncOperationHandle<IResourceLocator> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (_isFirstLaunch && !_isNetworkReachable)
                {
                    // use internal assets
                    _assetProviderSource.SetResult();
                    return;
                }

                // load external assets
                //Addressables.ClearResourceLocators();

                _loadCatalogHandle = Addressables.LoadContentCatalogAsync(_remoteCatalogPath, true);
                _loadCatalogHandle.Completed += OnLoadContentAction;
            }
            else
            {
                Log.Error($"init, failed to init addressables {handle.OperationException}");
                _assetProviderSource.SetResult();
            }
        }

        private void OnLoadContentAction(AsyncOperationHandle<IResourceLocator> loadCatalogHandle)
        {
            if (loadCatalogHandle.Status == AsyncOperationStatus.Succeeded)
            {
                // var locators = new List<IResourceLocator>(Addressables.ResourceLocators);
                // foreach (var loc in locators)
                // {
                //     if (loc.LocatorId != _remoteCatalogPath && 
                //         loc.LocatorId != "DynamicResourceLocator")
                //     {
                //         Addressables.RemoveResourceLocator(loc);    
                //     }
                // }

                Log.Debug($"init, on load content action {_remoteCatalogPath}");

                _downloadDepsHandle = Addressables.DownloadDependenciesAsync(
                    _labelsToDownload, Addressables.MergeMode.Union, true);
                _downloadDepsHandle.Completed += OnDownloadDepsAction;

                // var catalogsToUpdate = new List<string>() { _remoteCatalogPath };
                // PlayerPrefs.SetString("actual_catalog", _remoteLoadDataJson);
                // _updateCatalogHandle = Addressables.UpdateCatalogs(catalogsToUpdate, true);
                // _updateCatalogHandle.Completed += OnUpdateContentAction;
            }
            else
            {
                Log.Error($"init, failed to load catalog {_remoteLoadDataJson}" +
                    $" {loadCatalogHandle.OperationException}");
                _assetProviderSource.SetResult();
            }
        }

        private void OnDownloadDepsAction(AsyncOperationHandle handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                Log.Debug($"init, on download content action {_remoteCatalogPath}");
            else
            {
                Log.Error($"init, failed to download action {_remoteLoadDataJson}" +
                    $" {handle.OperationException}");
            }

            var catalogsToUpdate = new List<string>
                { _remoteCatalogPath };
            _updateCatalogHandle = Addressables.UpdateCatalogs(catalogsToUpdate);
            _updateCatalogHandle.Completed += OnUpdateContentAction;

            // can raise exception
            PlayerPrefs.SetString("actual_catalog", _remoteLoadDataJson);
        }

        private void OnUpdateContentAction(AsyncOperationHandle<List<IResourceLocator>> updateCatalogHandle)
        {
            if (updateCatalogHandle.Status == AsyncOperationStatus.Succeeded)
                Log.Debug($"init, catalog was updated {_remoteCatalogPath}");
            else
            {
                Log.Error($"init, failed to update catalog {_remoteCatalogPath}" +
                    $" {updateCatalogHandle.OperationException}");
            }

            _assetProviderSource.SetResult();
        }

        private async Awaitable<AddressablesRemoteConfig.AddressablesRemoteEntry>
            GetAssetBundleURL(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            // 2 retry
            var res = await GetAssetBundleURLImpl(url);
            if (res.Item1 == null && res.Item2)
            {
                Awaitable.WaitForSecondsAsync(1);
                res = await GetAssetBundleURLImpl(url);
            }

            var actualCatalog = PlayerPrefs.GetString("actual_catalog");
            _isFirstLaunch = string.IsNullOrEmpty(actualCatalog);

            if (res.Item1 == null)
            {
                // last chance
                if (!string.IsNullOrEmpty(actualCatalog))
                {
                    try
                    {
                        res = new(
                            JsonConvert.DeserializeObject<
                                AddressablesRemoteConfig.AddressablesRemoteEntry>(actualCatalog), false);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"init, failed to unpack actual catalog {actualCatalog} {ex}");
                    }
                }
            }
            return res.Item1;
        }

        private async Awaitable<Tuple<AddressablesRemoteConfig.AddressablesRemoteEntry, bool>> GetAssetBundleURLImpl(string url)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 5; // sec
                var operation = request.SendWebRequest();
                while (!operation.isDone) await Awaitable.NextFrameAsync();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Log.Error($"error downloading file: {request.error}");
                    return new(null, true);
                }

                var bytes = request.downloadHandler.text;

                try
                {
                    var remoteConfig = JsonConvert.DeserializeObject<AddressablesRemoteConfig>(bytes);
                    var entry = remoteConfig.Entries.FirstOrDefault(
                        x => x.version == Application.version);
                    return new(entry, false);
                }
                catch (Exception ex)
                {
                    Log.Error($"failed to unpack json {bytes} {ex}");
                }

                return new(null, true);
            }
        }

        ~LightweightAssetProvider() => ResourceManager.ExceptionHandler -= ExceptionHandler;

        private static void ExceptionHandler(AsyncOperationHandle handle, Exception exception)
        {
            if (exception.GetType() != typeof(InvalidKeyException)) Addressables.LogException(handle, exception);
        }

#region IAssetProvider
        public Awaitable<GameObject> Get(AssetReference reference, CancellationToken cancellationToken = default) => Get<GameObject>(reference, cancellationToken);
        public async Awaitable<T> Get<T>(AssetReference reference, CancellationToken cancellationToken = default) where T : Object
        {
            Assert.IsTrue(reference.RuntimeKeyIsValid());

            if (_cache.TryGetValue(reference.RuntimeKey.ToString(), out var handle))
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    if (handle.Result is T result) return result;

                    Log.Error($"Cached asset is not of type {typeof(T)}: {reference.RuntimeKey}, be aware that asset is loaded as {handle.Result.GetType()}");

                    return null;
                }

                await handle.Task;
            }
            else
            {
                handle = Addressables.LoadAssetAsync<Object>(reference);

                // as Addressables does not support cancellation token, we need to check it manually
                if (cancellationToken.IsCancellationRequested)
                {
                    Addressables.Release(handle);

                    return null;
                }

                _cache.Add(reference.RuntimeKey.ToString(), handle);

                await handle.Task;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded) return handle.Result as T;

            Log.Error($"AssetOperationHandleStorage.Get: Failed to load asset: {reference.RuntimeKey}, status: {handle.Status}");

            return null;
        }

        public async Awaitable<GameObject> Instantiate(AssetReference reference, Transform parent = null, CancellationToken cancellationToken = default)
        {
            var origin = await Get<GameObject>(reference, cancellationToken);
            if (origin == null)
            {
                Log.Error($"AssetOperationHandleStorage.Instantiate: Failed to load asset: {reference.RuntimeKey}");

                return null;
            }

            /*
            var result = await Object.InstantiateAsync(origin, parent);
            return result[0];
            */

            var result = await Object.InstantiateAsync(origin, parent);
            return result.FirstOrDefault();
        }

        public void ReleaseFor(AssetReference reference)
        {
            if (!_cache.TryGetValue(reference.RuntimeKey.ToString(), out var handle))
            {
                Log.Warning($"Requested to release asset that is not loaded: {reference.RuntimeKey}");

                return;
            }

            Addressables.Release(handle);
            _cache.Remove(reference.RuntimeKey.ToString());
        }

        public void LogVerbatimState()
        {
            var sb = new StringBuilder();
            sb.Append($"<color=red>LightweightAssetProvider has {_cache.Count} loaded entries: </color><color=white>");

            foreach (var item in _cache)
                sb.Append($"<color=yellow>{item.Key}</color> <b>Name:</b>{item.Value.DebugName}; <b>Status:</b>{item.Value.Status}; <b>Done:</b>{item.Value.IsDone}; ");

            sb.Append("</color>");
            Log.Debug(sb);
        }
#endregion
    }
}