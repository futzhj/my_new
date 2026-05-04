using System;
using System.Collections.Generic;

namespace Spine.Collections;

public static class CollectionExtensions
{
	public static OrderedDictionary<TKey, TSource> ToOrderedDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return source.ToOrderedDictionary(keySelector, null);
	}

	public static OrderedDictionary<TKey, TSource> ToOrderedDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		OrderedDictionary<TKey, TSource> orderedDictionary = new OrderedDictionary<TKey, TSource>(comparer);
		foreach (TSource item in source)
		{
			TKey key = keySelector(item);
			orderedDictionary.Add(key, item);
		}
		return orderedDictionary;
	}
}
