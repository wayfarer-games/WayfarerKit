using System;
using WayfarerKit.Patterns.EventBus;

namespace WayfarerKit.Patterns.Board
{
    [Serializable]
    public class BoardEntry<T>
    {
        public BoardEntry(BoardKey key, T value)
        {
            Key = key;
            Value = value;
            ValueType = typeof(T);
        }

        private BoardEntry() {}

        public BoardKey Key { get; }
        public T Value { get; }
        public Type ValueType { get; }

        public override bool Equals(object obj) => obj is BoardEntry<T> other && Key == other.Key;
        public override int GetHashCode() => Key.GetHashCode();
    }
}