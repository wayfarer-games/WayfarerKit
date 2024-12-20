#if UNITY_EDITOR
using UnityEngine;

namespace WayfarerKit.Editor.Helpers
{
    [CreateAssetMenu(fileName = "BundleVersionConfig", menuName = "WayfarerSDK/Addressables/Bundle Version Config", order = 1)]
    public class BundleVersionConfig : ScriptableObject
    {
        [Tooltip("URL template for asset bundles. Use [BuildTarget] for the platform placeholder and [Version] for the version.")]
        public string urlTemplate = "https://storage.googleapis.com/bundle-stg1.crs.wayfarer.studio/asset-bundles/[BuildTarget]/[Version]";
    
        [Tooltip("Current version of the asset bundles.")]
        public int version = 1;
        
        [Tooltip("GCS bucket path for uploading files.")]
        public string gcsPath = "gs://bundle-stg1.crs.wayfarer.studio/asset-bundles";

        /// <summary>
        /// Gets the resolved URL for the current version and build target.
        /// </summary>
        /// <param name="buildTarget">The build target to use in the URL.</param>
        /// <returns>Resolved URL.</returns>
        public string GetResolvedUrl(string buildTarget)
        {
            return urlTemplate.Replace("[BuildTarget]", buildTarget).Replace("[Version]", version.ToString());
        }
    }
}

#endif