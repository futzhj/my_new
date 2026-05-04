using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Spine.Collections;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(OrderedDictionaryDebugView<, >))]
public sealed class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IList<KeyValuePair<TKey, TValue>>
{
	public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, IEnumerable
	{
		private readonly Dictionary<TKey, int> dictionary;

		public int Count => dictionary.Count;

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool ICollection<TKey>.IsReadOnly => true;

		internal KeyCollection(Dictionary<TKey, int> dictionary)
		{
			this.dictionary = dictionary;
		}

		public void CopyTo(TKey[] array, int arrayIndex)
		{
			dictionary.Keys.CopyTo(array, arrayIndex);
		}

		public IEnumerator<TKey> GetEnumerator()
		{
			return dictionary.Keys.GetEnumerator();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool ICollection<TKey>.Contains(TKey item)
		{
			return dictionary.ContainsKey(item);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		void ICollection<TKey>.Add(TKey item)
		{
			throw new NotSupportedException("An attempt was made to edit a read-only list.");
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		void ICollection<TKey>.Clear()
		{
			throw new NotSupportedException("An attempt was made to edit a read-only list.");
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool ICollection<TKey>.Remove(TKey item)
		{
			throw new NotSupportedException("An attempt was made to edit a read-only list.");
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, IEnumerable
	{
		private readonly List<TValue> values;

		public int Count => values.Count;

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool ICollection<TValue>.IsReadOnly => true;

		internal ValueCollection(List<TValue> values)
		{
			this.values = values;
		}

		public void CopyTo(TValue[] array, int arrayIndex)
		{
			values.CopyTo(array, arrayIndex);
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			return values.GetEnumerator();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool ICollection<TValue>.Contains(TValue item)
		{
			return values.Contains(item);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		void ICollection<TValue>.Add(TValue item)
		{
			throw new NotSupportedException("An attempt was made to edit a read-only list.");
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		void ICollection<TValue>.Clear()
		{
			throw new NotSupportedException("An attempt was made to edit a read-only list.");
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool ICollection<TValue>.Remove(TValue item)
		{
			throw new NotSupportedException("An attempt was made to edit a read-only list.");
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private readonly Dictionary<TKey, int> dictionary;

	private readonly List<TKey> keys;

	private readonly List<TValue> values;

	private int version;

	private const string CollectionModifiedMessage = "Collection was modified; enumeration operation may not execute.";

	private const string EditReadOnlyListMessage = "An attempt was made to edit a read-only list.";

	private const string IndexOutOfRangeMessage = "The index is negative or outside the bounds of the collection.";

	public IEqualityComparer<TKey> Comparer => dictionary.Comparer;

	public KeyCollection Keys => new KeyCollection(dictionary);

	public ValueCollection Values => new ValueCollection(values);

	public TValue this[int index]
	{
		get
		{
			return values[index];
		}
		set
		{
			values[index] = value;
		}
	}

	public TValue this[TKey key]
	{
		get
		{
			return values[dictionary[key]];
		}
		set
		{
			if (dictionary.TryGetValue(key, out var value2))
			{
				keys[value2] = key;
				values[value2] = value;
			}
			else
			{
				Add(key, value);
			}
		}
	}

	public int Count => dictionary.Count;

	KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
	{
		get
		{
			TKey key = keys[index];
			TValue value = values[index];
			return new KeyValuePair<TKey, TValue>(key, value);
		}
		set
		{
			TKey val = keys[index];
			if (dictionary.Comparer.Equals(val, value.Key))
			{
				dictionary[value.Key] = index;
			}
			else
			{
				dictionary.Add(value.Key, index);
				dictionary.Remove(val);
			}
			keys[index] = value.Key;
			values[index] = value.Value;
		}
	}

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

	ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

	public OrderedDictionary()
		: this(0, (IEqualityComparer<TKey>)null)
	{
	}

	public OrderedDictionary(int capacity)
		: this(capacity, (IEqualityComparer<TKey>)null)
	{
	}

	public OrderedDictionary(IEqualityComparer<TKey> comparer)
		: this(0, comparer)
	{
	}

	public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
	{
		dictionary = new Dictionary<TKey, int>(capacity, comparer ?? EqualityComparer<TKey>.Default);
		keys = new List<TKey>(capacity);
		values = new List<TValue>(capacity);
	}

	public void Add(TKey key, TValue value)
	{
		dictionary.Add(key, values.Count);
		keys.Add(key);
		values.Add(value);
		version++;
	}

	public void Insert(int index, TKey key, TValue value)
	{
		if (index < 0 || index > values.Count)
		{
			throw new ArgumentOutOfRangeException("index", index, "The index is negative or outside the bounds of the collection.");
		}
		dictionary.Add(key, index);
		for (int i = index; i != keys.Count; i++)
		{
			TKey key2 = keys[i];
			dictionary[key2]++;
		}
		keys.Insert(index, key);
		values.Insert(index, value);
		version++;
	}

	public bool ContainsKey(TKey key)
	{
		return dictionary.ContainsKey(key);
	}

	public TKey GetKey(int index)
	{
		return keys[index];
	}

	public int IndexOf(TKey key)
	{
		if (dictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		return -1;
	}

	public bool Remove(TKey key)
	{
		if (dictionary.TryGetValue(key, out var value))
		{
			RemoveAt(value);
			return true;
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		TKey key = keys[index];
		for (int i = index + 1; i < keys.Count; i++)
		{
			TKey key2 = keys[i];
			dictionary[key2]--;
		}
		dictionary.Remove(key);
		keys.RemoveAt(index);
		values.RemoveAt(index);
		version++;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		if (dictionary.TryGetValue(key, out var value2))
		{
			value = values[value2];
			return true;
		}
		value = default(TValue);
		return false;
	}

	public void Clear()
	{
		dictionary.Clear();
		keys.Clear();
		values.Clear();
		version++;
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		int startVersion = version;
		int index = 0;
		while (index != keys.Count)
		{
			TKey key = keys[index];
			TValue value = values[index];
			yield return new KeyValuePair<TKey, TValue>(key, value);
			if (version != startVersion)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			int num = index + 1;
			index = num;
		}
	}

	int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item)
	{
		if (dictionary.TryGetValue(item.Key, out var value) && object.Equals(values[value], item.Value))
		{
			return value;
		}
		return -1;
	}

	void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
	{
		Insert(index, item.Key, item.Value);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
	{
		Add(item.Key, item.Value);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
	{
		if (dictionary.TryGetValue(item.Key, out var value) && object.Equals(values[value], item.Value))
		{
			return true;
		}
		return false;
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, "The index is negative or outside the bounds of the collection.");
		}
		int num = 0;
		while (num != keys.Count && arrayIndex < array.Length)
		{
			TKey key = keys[num];
			TValue value = values[num];
			array[arrayIndex] = new KeyValuePair<TKey, TValue>(key, value);
			num++;
			arrayIndex++;
		}
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
	{
		if (((ICollection<KeyValuePair<TKey, TValue>>)this).Contains(item))
		{
			return Remove(item.Key);
		}
		return false;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
