using UnityEngine;

namespace WayfarerKit.Extensions
{
    public static class UnityObjectExtensions
    {
        public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;
    }
}