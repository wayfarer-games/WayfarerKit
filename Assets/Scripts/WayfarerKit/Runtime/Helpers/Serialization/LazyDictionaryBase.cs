using System;

namespace WayfarerKit.Helpers.Serialization
{
	/// <summary>
	///     Class that stores values and their constructor to create them the first
	///     time they are used.
	/// </summary>
	public abstract class LazyDictionaryBase<TKey, TValue>
    {
        public TValue this[TKey key]
        {
            get
            {
                try
                {
                    return GetValue(key);
                }
                catch (Exception e)
                {
                    throw new($"Failed returning item for key {key}.", e);
                }
            }
        }

        protected abstract TValue GetValue(TKey key);
    }
}