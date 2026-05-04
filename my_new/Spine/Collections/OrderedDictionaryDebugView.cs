using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Spine.Collections;

internal class OrderedDictionaryDebugView<TKey, TValue>
{
	private readonly OrderedDictionary<TKey, TValue> dictionary;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public KeyValuePair<TKey, TValue>[] Items => dictionary.ToArray();

	public OrderedDictionaryDebugView(OrderedDictionary<TKey, TValue> dictionary)
	{
		this.dictionary = dictionary;
	}
}
