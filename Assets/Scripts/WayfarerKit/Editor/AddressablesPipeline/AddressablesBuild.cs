#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace WayfarerKit.Editor.Helpers
{
    internal class BuildLauncher
    {
        private const string DefaultProfileName = "Default";

        private const string AddressablesConfigPath = "Assets/Resources/AddressablesConfig.asset";
        private const string BuildScript = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
        private const string SettingsAsset = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        private const string ServerDataPath = "ServerData";

        private static AddressableAssetSettings _settings;

        private static void GetSettingsObject(string settingsAsset)
        {
            _settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset) as AddressableAssetSettings;

            if (_settings == null)
                Debug.LogError($"{settingsAsset} couldn't be found or isn't a settings object.");
        }

        private static void SetProfile(string profileName)
        {
            var profileId = _settings.profileSettings.GetProfileId(profileName);
            if (string.IsNullOrEmpty(profileId))
                Debug.LogWarning($"Couldn't find a profile named, {profileName}, using current profile instead.");
            else
                _settings.activeProfileId = profileId;
        }

        private static void SetBuilder(IDataBuilder builder)
        {
            var index = _settings.DataBuilders.IndexOf((ScriptableObject)builder);

            if (index > 0)
                _settings.ActivePlayerDataBuilderIndex = index;
            else
                Debug.LogWarning($"{builder} must be added to the DataBuilders list before it can be made active. Using last run builder instead.");
        }

        private static bool BuildAddressableContent()
        {
            AddressableAssetSettings.BuildPlayerContent(out var result);
            bool success = string.IsNullOrEmpty(result.Error);

            if (!success)
            {
                Debug.LogError("Addressables build error encountered: " + result.Error);
            }

            return success;
        }

        [MenuItem("WayfarerSDK/Tools/Addressables/Build Remote Addressables", false, 3)]
        public static bool BuildAddressables()
        {
            GetSettingsObject(SettingsAsset);
            
            var config = AssetDatabase.LoadAssetAtPath<Runtime.Addressables.AddressablesConfig>(AddressablesConfigPath);
            if (config == null)
            {
                UnityEngine.Debug.LogError($"AddressablesConfig not found at path: {AddressablesConfigPath}");
                return false;
            }

            var profileName = config.IsProd ? config.ProfileProd : config.ProfileStg;
            SetProfile(profileName);

            var builderScript = AssetDatabase.LoadAssetAtPath<ScriptableObject>(BuildScript) as IDataBuilder;
            if (builderScript == null)
            {
                Debug.LogError(BuildScript + " couldn't be found or isn't a build script.");
                return false;
            }

            var buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
            var buildPath = $"{ServerDataPath}/{buildTarget}";
            CleanFolder(buildPath);
            SetBuilder(builderScript);

            var buildSuccess = BuildAddressableContent();

            SetProfile(DefaultProfileName);

            return buildSuccess;
        }

        private static void CleanFolder(string folderPath)
        {
            var files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"failed to clean folder: {folderPath} {ex}");
                }
            }
        }
            

        // [MenuItem("Window/Asset Management/Addressables/Build Addressables and Player")]
        // public static void BuildAddressablesAndPlayer()
        // {
        //     bool contentBuildSucceeded = BuildAddressables();
        //
        //     if (contentBuildSucceeded)
        //     {
        //         var options = new BuildPlayerOptions();
        //         BuildPlayerOptions playerSettings
        //             = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(options);
        //
        //         BuildPipeline.BuildPlayer(playerSettings);
        //     }
        // }
    }
#endif
}