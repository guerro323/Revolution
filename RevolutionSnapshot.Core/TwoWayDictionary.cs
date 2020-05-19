using Collections.Pooled;

namespace RevolutionSnapshot.Core
{
	public class TwoWayDictionary<TKey, TValue>
	{
		public PooledDictionary<TKey, TValue> ToValue;
		public PooledDictionary<TValue, TKey> ToKey;

		public TwoWayDictionary(int capacity = 0)
		{
			ToValue = new PooledDictionary<TKey, TValue>(capacity);
			ToKey   = new PooledDictionary<TValue, TKey>(capacity);
		}

		public void Set(TKey key, TValue value)
		{
			ToValue[key] = value;
			ToKey[value] = key;
		}

		public bool TryGetValue(TKey key,   out TValue value) => ToValue.TryGetValue(key, out value);
		public bool TryGetKey(TValue value, out TKey   key)   => ToKey.TryGetValue(value, out key);

		public TValue GetValue(TKey key)   => ToValue[key];
		public TKey   GetKey(TValue value) => ToKey[value];
	}
}