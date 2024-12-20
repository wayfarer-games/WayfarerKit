#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace WayfarerKit.Editor.Helpers
{
    public static class AddressablesVersionManager
    {
        private const string AddressablesConfigPath = "Assets/Resources/AddressablesConfig.asset";
        private const string ConfigDebugPath = "Assets/Resources/Editor/BundleVersionConfigDebug.asset";
        private const string ConfigProdPath = "Assets/Resources/Editor/BundleVersionConfigProd.asset";

        [MenuItem("WayfarerSDK/Tools/Addressables/Increment Addressables Build Version", false, 1)]
        public static void IncrementBuildVersion()
        {
            var config = AssetDatabase.LoadAssetAtPath<Runtime.Addressables.AddressablesConfig>(AddressablesConfigPath);
            if (config == null)
            {
                UnityEngine.Debug.LogError($"AddressablesConfig not found at path: {AddressablesConfigPath}");
                return;
            }

            var configPath = config.IsProd ? ConfigProdPath : ConfigDebugPath;
            
            var config2 = AssetDatabase.LoadAssetAtPath<BundleVersionConfig>(configPath);
            if (config2 == null)
            {
                Debug.LogError($"BundleVersionConfig not found at path: {configPath}");
                return;
            }

            config2.version++;
            EditorUtility.SetDirty(config2);
            AssetDatabase.SaveAssets();
            Debug.Log($"Incremented bundle version to {config2.version}");
        }

        [MenuItem("WayfarerSDK/Tools/Addressables/Update Addressables Profile Version", false, 2)]
        public static void UpdateAddressablesProfileVersion()
        {
            var config = AssetDatabase.LoadAssetAtPath<Runtime.Addressables.AddressablesConfig>(AddressablesConfigPath);
            if (config == null)
            {
                UnityEngine.Debug.LogError($"AddressablesConfig not found at path: {AddressablesConfigPath}");
                return;
            }

            var configPath = config.IsProd ? ConfigProdPath : ConfigDebugPath;
            
            var config2 = AssetDatabase.LoadAssetAtPath<BundleVersionConfig>(configPath);
            if (config2 == null)
            {
                Debug.LogError($"BundleVersionConfig not found at path: {configPath}");
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found. Please set up Addressables in your project.");
                return;
            }

            var profileName = config.IsProd ? config.ProfileProd : config.ProfileStg;
            var profileId = settings.profileSettings.GetProfileId(profileName);
            settings.profileSettings.SetValue(profileId, "Remote.LoadPath", 
                config2.urlTemplate.Replace("[Version]", config2.version.ToString()));

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log($"Updated Addressables profile to version {profileName} {config2.version}");
        }
    }
}

#endif