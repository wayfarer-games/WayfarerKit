using System.Linq;

namespace WayfarerKit.Helpers.Hashing
{
    public static class Fnv1A
    {
        public static int ComputeFnv1A(this string source)
        {
            var hash = source.Aggregate(2166136261, (current, c) => (current ^ c) * 16777619);

            return unchecked((int)hash);
        }

        public static int GetHashCode(params object[] values)
        {
            const int fnvPrime = 16777619;
            const int offsetBasis = unchecked((int)2166136261);

            var hash = offsetBasis;
            foreach (var value in values)
            {
                if (value == null) continue;
                var strValue = value.ToString();
                foreach (var c in strValue)
                {
                    hash ^= c;
                    hash *= fnvPrime;
                }
            }

            return hash;
        }
    }
}