using System;

namespace WayfarerKit.Patterns.Singletons.Helpers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScriptableObjectPathAttribute : Attribute
    {
        public ScriptableObjectPathAttribute(string path) => Path = path;
        public string Path { get; }
    }
}