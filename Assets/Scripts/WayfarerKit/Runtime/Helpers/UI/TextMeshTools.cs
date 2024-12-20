using Unity.Logging;
using UnityEngine.Localization;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;

namespace WayfarerKit.Helpers.UI
{
    public static class TextMeshTools
    {
        public static void SetLocalizedString(this GameObjectLocalizer localizer, LocalizedString localizedString)
        {
            if (localizer.TrackedObjects == null || localizer.TrackedObjects.Count == 0)
            {
                Log.Error($"No tracked objects found in localizer {localizer.name}");
                return;
            }

            var variant = localizer.TrackedObjects[0].GetTrackedProperty<LocalizedStringProperty>("m_text");
            if (variant != null) variant.LocalizedString = localizedString;
        }

        public static LocalizedString GetLocalizedStringAtIndex(this GameObjectLocalizer localizer, int index = 0)
        {
            if (localizer.TrackedObjects == null || localizer.TrackedObjects.Count == 0)
            {
                Log.Error($"No tracked objects found in localizer {localizer.name}");
                return null;
            }
            
            var variant = localizer.TrackedObjects[0].GetTrackedProperty<LocalizedStringProperty>("m_text");
            return variant?.LocalizedString;
        }
    }
}