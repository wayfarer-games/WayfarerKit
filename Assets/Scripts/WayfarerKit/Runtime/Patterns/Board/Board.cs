using System;
using System.Collections.Generic;
using Unity.Logging;
using UnityEngine;
using WayfarerKit.Patterns.EventBus;

namespace WayfarerKit.Patterns.Board
{
    [Serializable]
    public class Board
    {
        private Dictionary<BoardKey, object> _entries = new();
        private Dictionary<string, BoardKey> _registry = new();

        public bool ContainsKey(BoardKey key) => _entries.ContainsKey(key);
        public BoardKey GetOrRegisterKey(string name)
        {
            if (_registry.TryGetValue(name, out var key)) return key;

            key = new(name);
            _registry.Add(name, key);

            return key;
        }

        public bool TryGetValue<T>(BoardKey key, out T value)
        {
            if (_entries.TryGetValue(key, out var entry) && entry is BoardEntry<T> casted)
            {
                value = casted.Value;

                return true;
            }

            value = default;

            return false;
        }

        public void SetValue<T>(BoardKey key, T value) => _entries[key] = new BoardEntry<T>(key, value);
        public bool RemoveKey(BoardKey key) => _entries.Remove(key);

        public void DebugVerbatimState()
        {
            foreach (var entry in _entries)
            {
                var entryType = entry.Value.GetType();

                if (!entryType.IsGenericType || entryType.GetGenericTypeDefinition() != typeof(BoardEntry<>))
                    continue;

                var valueProperty = entryType.GetProperty("Value");

                if (valueProperty == null)
                    continue;

                var value = valueProperty.GetValue(entry.Value);
                Log.Debug($"<color=red>Blackboard</color> Key: <color=yellow>{entry.Key}</color>, Value: <color=yellow>{value}</color>");
            }
        }
    }
}