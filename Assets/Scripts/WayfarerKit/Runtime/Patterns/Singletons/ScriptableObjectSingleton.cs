using System;
using System.IO;
using System.Linq;
using UnityEngine;
using WayfarerKit.Patterns.Singletons.Helpers;

namespace WayfarerKit.Patterns.Singletons
{
    /// <summary>
    ///     Use ScriptableObjectPathAttribute to set the path of the ScriptableObject in Resources folder.
    /// </summary>
    public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;

        // ReSharper disable once StaticMemberInGenericType
        [NonSerialized] private static string _path;

        private static string Path
        {
            get
            {
                if (!string.IsNullOrEmpty(_path)) return _path;

                if (typeof(T).GetCustomAttributes(typeof(ScriptableObjectPathAttribute), false).FirstOrDefault() is not ScriptableObjectPathAttribute attribute) throw new InvalidOperationException($"ScriptableObjectSingleton.Path: Path for {typeof(T).Name} is not set. Make sure to set the path using ScriptableObjectPathAttribute.");

                _path = attribute.Path;

                return _path;
            }
        }

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = Resources.Load<T>(Path);

                if (_instance == null) throw new FileNotFoundException($"Cannot find Scriptable Object of type {typeof(T).Name} at path {Path}.");

                return _instance;
            }
        }
    }
}