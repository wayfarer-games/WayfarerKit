using System;
using UnityEngine;

namespace WayfarerKit.Helpers.Serialization
{
    [Serializable]
    public struct Optional<T>
    {
        [SerializeField] private bool enabled;
        [SerializeField] private T value;

        public bool Enabled => enabled;
        public T Value => value;

        public Optional(T initialValue)
        {
            enabled = true;
            value = initialValue;
        }

        public static implicit operator T(Optional<T> optional) => optional.value;
    }
}