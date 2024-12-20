#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace WayfarerKit.Editor.Helpers
{
    public class BundleMappingManager
    {
        private const string AddressablesConfigPath = "Assets/Resources/AddressablesConfig.asset";
        private const string BundleMappingDebugPath = "Assets/AddressableAssetsData/BundlesMapping/Debug";
        private const string BundleMappingProdPath = "Assets/AddressableAssetsData/BundlesMapping/Prod";
        private const string BundleConfigDebugPath = "Assets/Resources/Editor/BundleVersionConfigDebug.asset";
        private const string BundleConfigProdPath = "Assets/Resources/Editor/BundleVersionConfigProd.asset";
        private const string ServerDataPath = "ServerData";

        [MenuItem("WayfarerSDK/Tools/Addressables/Build Addressables Mapping", false, 4)]
        public static void BuildAddressablesMapping()
        {
            CreateBundleMappingFile();
        }

        private static void CreateBundleMappingFile()
        {
            // Load the BundleVersionConfig asset
            var config = AssetDatabase.LoadAssetAtPath<Runtime.Addressables.AddressablesConfig>(AddressablesConfigPath);
            if (config == null)
            {
                UnityEngine.Debug.LogError($"AddressablesConfig not found at path: {AddressablesConfigPath}");
                return;
            }

            var bundleMappingPath = config.IsProd ? BundleMappingProdPath : BundleMappingDebugPath;
            
            var buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
            var version = GetNextVersion(config);
            var newMappingPath = $"{bundleMappingPath}/map_{buildTarget.ToLower()}_{version}.json";

            var previousMappingPath = GetPreviousMappingPath(config, buildTarget);

            var entries = new List<BundleEntry>();
            if (File.Exists(previousMappingPath))
            {
                var previousContent = File.ReadAllText(previousMappingPath);
                entries = JsonConvert.DeserializeObject<BundleMapping>(previousContent)?.Entries ?? new List<BundleEntry>();
                entries.RemoveAll(item => item.version == Application.version);
            }

            var buildPath = $"{ServerDataPath}/{buildTarget}";
            var latestCatalog = GetLatestFile(buildPath, "*.bin");
            var latestHash = GetLatestFile(buildPath, "*.hash");
            var latestBundle = GetLatestFile(buildPath, "*.bundle");

            if (string.IsNullOrEmpty(latestCatalog) || string.IsNullOrEmpty(latestHash) || string.IsNullOrEmpty(latestBundle))
            {
                Debug.LogError("Failed to find the latest catalog, hash, or bundle files.");
                return;
            }

            var catalogName = Path.GetFileNameWithoutExtension(latestCatalog);
            
            entries.Add(new BundleEntry
            {
                version = Application.version,
                assetsURL = GetURL(config, buildTarget, version),
                catalogName = catalogName,
                bundleName = Path.GetFileName(latestBundle)
            });

            var mapping = new BundleMapping { Entries = entries };
            Directory.CreateDirectory(bundleMappingPath);
            File.WriteAllText(newMappingPath, JsonConvert.SerializeObject(mapping, Formatting.Indented));
            Debug.Log($"Created new mapping file: {newMappingPath}");
        }

        private static int GetNextVersion(Runtime.Addressables.AddressablesConfig config)
        {
            var bundleConfigPath = config.IsProd ? BundleConfigProdPath : BundleConfigDebugPath;
            var config2 = AssetDatabase.LoadAssetAtPath<BundleVersionConfig>(bundleConfigPath);
            if (config2 == null)
            {
                Debug.LogError("BundleVersionConfig not found at specified path.");
                return 1;
            }
            return config2.version;
        }

        private static string GetURL(Runtime.Addressables.AddressablesConfig config, string buildTarget, int version)
        {
            var bundleConfigPath = config.IsProd ? BundleConfigProdPath : BundleConfigDebugPath;
            var config2 = AssetDatabase.LoadAssetAtPath<BundleVersionConfig>(bundleConfigPath);
            if (config2 == null)
            {
                Debug.LogError("BundleVersionConfig not found at specified path.");
                return string.Empty;
            }
            return config2.GetResolvedUrl(buildTarget).Replace("[Version]", version.ToString());
        }

        private static string GetPreviousMappingPath(Runtime.Addressables.AddressablesConfig config, string buildTarget)
        {
            var mappingPath = config.IsProd ? BundleMappingProdPath : BundleMappingDebugPath;
            if (!Directory.Exists(mappingPath))
                return string.Empty;

            var files = Directory.GetFiles(mappingPath, $"map_{buildTarget.ToLower()}_*.json");
            if (files.Length == 0) return string.Empty;

            var sortedFiles = files
                .Select(f =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(f);
                    string[] parts = fileName.Split('_');
                    if (parts.Length < 3 || !int.TryParse(parts[2], out int version))
                    {
                        return (file: f, version: -1);
                    }
                    return (file: f, version: version);
                })
                .Where(f => f.version >= 0)
                .OrderByDescending(f => f.version)
                .Select(f => f.file)
                .ToList();

            return sortedFiles.FirstOrDefault() ?? string.Empty;
        }

        private static string GetLatestFile(string folder, string searchPattern)
        {
            var files = Directory.GetFiles(folder, searchPattern);
            return files.OrderByDescending(f => File.GetLastWriteTime(f)).FirstOrDefault();
        }

        [Serializable, Preserve]
        public class BundleMapping
        {
            public List<BundleEntry> Entries;
        }

        [Serializable, Preserve]
        public class BundleEntry
        {
            public string version;
            public string assetsURL;
            public string catalogName;
            public string bundleName;
        }
    }
}
#endif