#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;

namespace WayfarerKit.Editor.Helpers
{
    public static class GCSUploader
    {
        private const string AddressablesConfigPath = "Assets/Resources/AddressablesConfig.asset";
        private const string BundleConfigDebugPath = "Assets/Resources/Editor/BundleVersionConfigDebug.asset";
        private const string BundleConfigProdPath = "Assets/Resources/Editor/BundleVersionConfigProd.asset";
        private const string ServerDataPath = "ServerData";
        private const string BundleMappingDebugPath = "Assets/AddressableAssetsData/BundlesMapping/Debug";
        private const string BundleMappingProdPath = "Assets/AddressableAssetsData/BundlesMapping/Prod";

        [MenuItem("WayfarerSDK/Tools/Addressables/Upload to GCS", false, 5)]
        public static void UploadToGCS()
        {
            var config = AssetDatabase.LoadAssetAtPath<Runtime.Addressables.AddressablesConfig>(AddressablesConfigPath);
            if (config == null)
            {
                UnityEngine.Debug.LogError($"AddressablesConfig not found at path: {AddressablesConfigPath}");
                return;
            }

            var bundleMappingPath = config.IsProd ? BundleMappingProdPath : BundleMappingDebugPath;
            var bundleConfigPath = config.IsProd ? BundleConfigProdPath : BundleConfigDebugPath;
            
            // Load the BundleVersionConfig asset
            var config2 = AssetDatabase.LoadAssetAtPath<BundleVersionConfig>(bundleConfigPath);
            if (config2 == null)
            {
                UnityEngine.Debug.LogError($"BundleVersionConfig not found at path: {bundleConfigPath}");
                return;
            }

            var buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
            var gcsPath = Path.Join(config2.gcsPath,buildTarget, config2.version.ToString());

            // Find the latest .bin, .hash, and .bundle files
            var buildPath = $"{ServerDataPath}/{buildTarget}";
            var latestBin = GetLatestFile(buildPath, "*.bin");
            var latestHash = GetLatestFile(buildPath, "*.hash");
            var latestBundle = GetLatestFile(buildPath, "*.bundle");

            if (string.IsNullOrEmpty(latestBin) || string.IsNullOrEmpty(latestHash) || string.IsNullOrEmpty(latestBundle))
            {
                UnityEngine.Debug.LogError("Failed to find required files (.bin, .hash, .bundle) in ServerData.");
                return;
            }

            // Find the mapping file
            var mappingFile = $"{bundleMappingPath}/map_{buildTarget.ToLower()}_{config2.version}.json";
            if (!File.Exists(mappingFile))
            {
                UnityEngine.Debug.LogError($"Mapping file not found: {mappingFile}");
                return;
            }

            // Upload files to GCS
            UploadFileToGCS(latestBin, gcsPath, Path.GetFileName(latestBin));
            UploadFileToGCS(latestHash, gcsPath, Path.GetFileName(latestHash));
            UploadFileToGCS(latestBundle, gcsPath, Path.GetFileName(latestBundle));
            UploadFileToGCS(mappingFile, config2.gcsPath, Path.GetFileName(mappingFile));

            UnityEngine.Debug.Log("All files uploaded successfully to GCS.");
        }

        private static string GetLatestFile(string folder, string searchPattern)
        {
            var files = Directory.GetFiles(folder, searchPattern);
            return files.OrderByDescending(File.GetLastWriteTime).FirstOrDefault();
        }

        private static void UploadFileToGCS(string localFilePath, string gcsBasePath, string destinationFileName)
        {
            var scriptPath = "./upload_file_to_gcs.sh";

            if (!File.Exists(scriptPath))
            {
                UnityEngine.Debug.LogError($"Bash script not found: {scriptPath}");
                return;
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"{scriptPath} {localFilePath} {gcsBasePath} {destinationFileName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
            
                if (process.ExitCode != 0)
                {
                    UnityEngine.Debug.LogError($"Error uploading file {localFilePath} to GCS: {output} {error}");
                }
                else
                {
                    UnityEngine.Debug.Log($"File uploaded successfully: {output}");
                }
            }
        }
    }
}
#endif