using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Logging;
using UnityEditor;
using UnityEngine;

namespace WayfarerKit.Patterns.EventBus.Helpers
{
    public static class EventBusRuntimeInitialize
    {
        private static IReadOnlyList<Type> EventTypes { get; set; }
        private static IReadOnlyList<Type> EventBusTypes { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            EventTypes = PredefinedAssemblyHelper.GetTypes(typeof(IBusEvent));
            EventBusTypes = GetAllBusTypes();
        }

        private static List<Type> GetAllBusTypes()
        {
            var eventBusTypes = new List<Type>();
            var typedef = typeof(EventBus<>);

            foreach (var eventType in EventTypes)
            {
                var busType = typedef.MakeGenericType(eventType);
                eventBusTypes.Add(busType);

                //Log.Debug($"EventBusRuntimeInitialize.GetAllBusTypes: <color=yellow>EventBus<{busType.GetGenericArguments()[0].Name}></color> was initialized");
            }

            return eventBusTypes;
        }


        private static void ClearAllBuses()
        {
            if (EventBusTypes == null || EventBusTypes.Count == 0) return;

            foreach (var busType in EventBusTypes)
            {
                var clearMethod = busType.GetMethod("Clear",
                    BindingFlags.Static | BindingFlags.NonPublic);

                if (clearMethod == null) throw new MissingMethodException($"EventBusRuntimeInitialize.ClearAllBuses: Clear method not found for <color=yellow>{busType.GetGenericArguments()[0].Name}</color>");

                clearMethod.Invoke(null, null);
            }
        }

#if UNITY_EDITOR
        private static bool _isEditorInitialized;

        [InitializeOnLoadMethod]
        public static void InitializeEditor()
        {
            if (_isEditorInitialized) return;

            _isEditorInitialized = true;

            EditorApplication.playModeStateChanged += EditorOnplayModeStateChanged;
        }
        private static void EditorOnplayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode) ClearAllBuses();
        }
#endif
    }
}