using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace WayfarerKit.Editor.Helpers
{
    public static class AddressablesExtensions
    {
        public static void RemoveGroupIfExists(string groupName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (!settings)
            {
                Debug.LogError($"AddressableAssetSettingsDefaultObject.Settings was null, can't delete group {groupName}.");

                return;
            }

            var group = settings.FindGroup(groupName);

            if (!group)
                return;

            settings.RemoveGroup(group);
        }

        public static void SetAddressableGroup(this Object obj, string groupName, string label = "")
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (!settings)
            {
                Debug.LogError($"AddressableAssetSettingsDefaultObject.Settings was null, can't set group {groupName} for {obj.name}.");

                return;
            }

            var group = settings.FindGroup(groupName);
            if (!group)
            {
                group = settings.CreateGroup(groupName, false, false, true, null,
                    typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
            }

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
            var entriesAdded = new List<AddressableAssetEntry>
            {
                settings.CreateOrMoveEntry(guid, group, false, false)
            };

            if (!string.IsNullOrEmpty(label))
            {
                entriesAdded[0].SetLabel(label, true, true, true);
            }
            group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false);
        }

        // /* ============= CODE WITH SEPARATE PACKING MODE =============  */
        // public static void SetAddressableGroup(this Object obj, string groupName)
        // {
        //     var settings = AddressableAssetSettingsDefaultObject.Settings;
        //     if (!settings)
        //     {
        //         Debug.LogError($"AddressableAssetSettingsDefaultObject.Settings was null, can't set group {groupName} for {obj.name}.");
        //
        //         return;
        //     }
        //
        //     var group = settings.FindGroup(groupName);
        //     if (!group)
        //     {
        //         group = settings.CreateGroup(groupName, false, false, true, null,
        //             typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
        //     }
        //
        //     var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
        //     var entry = settings.CreateOrMoveEntry(guid, group, false, false);
        //     var entriesAdded = new List<AddressableAssetEntry> { entry };
        //     
        //     var bundledAssetGroupSchema = group.GetSchema<BundledAssetGroupSchema>();
        //     if (bundledAssetGroupSchema != null)
        //         bundledAssetGroupSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
        //     else
        //     {
        //         bundledAssetGroupSchema = group.AddSchema<BundledAssetGroupSchema>();
        //         bundledAssetGroupSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
        //     }
        //
        //     group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
        //     settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true);
        // }
    }
}