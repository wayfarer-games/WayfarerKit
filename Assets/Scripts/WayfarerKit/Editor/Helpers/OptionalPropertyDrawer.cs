using UnityEditor;
using UnityEngine;
using WayfarerKit.Helpers.Serialization;

namespace WayfarerKit.Editor.Helpers
{
    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalPropertyDrawer : PropertyDrawer
    {
        private const float ToggleWidth = 24;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var enabledProperty = property.FindPropertyRelative("enabled");
            var valueProperty = property.FindPropertyRelative("value");

            position.width -= ToggleWidth;
            EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);
            EditorGUI.PropertyField(position, valueProperty, label, true);
            EditorGUI.EndDisabledGroup();

            position.x += position.width + ToggleWidth;
            position.width = position.height = EditorGUI.GetPropertyHeight(enabledProperty);
            position.x -= position.width;
            EditorGUI.PropertyField(position, enabledProperty, GUIContent.none);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative("value");

            return EditorGUI.GetPropertyHeight(valueProperty);
        }
    }
}