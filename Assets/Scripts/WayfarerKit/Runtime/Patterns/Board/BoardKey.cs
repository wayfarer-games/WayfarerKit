using System;
using WayfarerKit.Helpers.Hashing;

namespace WayfarerKit.Patterns.EventBus
{
    [Serializable]
    public readonly struct BoardKey : IEquatable<BoardKey>
    {
        private readonly string _name;
        private readonly int _hash;

        public BoardKey(string name)
        {
            _name = name;
            _hash = name.ComputeFnv1A();
        }

        public static bool operator ==(BoardKey left, BoardKey right) => left._hash == right._hash;
        public static bool operator !=(BoardKey left, BoardKey right) => left._hash != right._hash;

        public bool Equals(BoardKey other) => _hash == other._hash;
        public override bool Equals(object obj) => obj is BoardKey other && Equals(other);
        public override int GetHashCode() => _hash;
        public override string ToString() => _name;
    }
}