using System;
using System.Collections.Generic;
using UnityEditor;
using WayfarerKit.Helpers.Serialization;

namespace WayfarerKit.Helpers
{
    public class SerializedPropertyCache : LazyDictionaryBase<string, SerializedProperty>
    {
        private const string PrivatePrefix = "_";

        private readonly Dictionary<string, SerializedProperty> _properties = new();

        public SerializedPropertyCache(SerializedObject serializedObject) =>
            SerializedObject = serializedObject ?? throw new("Serialized object can't be null");
        public SerializedObject SerializedObject { get; }

        public void UpdateRepresentation() => SerializedObject.Update();
        public virtual void ApplyChanges() => SerializedObject.ApplyModifiedProperties();

        protected override SerializedProperty GetValue(string nameOfProperty)
        {
            // Null or empty property names aren't allowed.
            if (string.IsNullOrEmpty(nameOfProperty))
                throw new NullReferenceException("Property name is null or empty.");

            if (_properties.TryGetValue(nameOfProperty, out var value))
                return value;

            var property = SerializedObject.FindProperty(nameOfProperty);
            if (property == null)
            {
                var privatePrefixed = PrivatePrefix + nameOfProperty;
                property = SerializedObject.FindProperty(privatePrefixed);

                if (property == null)
                    throw new InvalidOperationException($"{nameOfProperty} and {privatePrefixed} don't match a property.");
            }

            _properties.Add(nameOfProperty, property);

            return _properties[nameOfProperty];
        }
    }
}