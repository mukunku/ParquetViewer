using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParquetViewer.Engine.Types
{
	public class MapValueCollection : IComparable<MapValueCollection>, IComparable
	{
		public readonly KeyedCollection<MapValue, MapValue> values = new OrderedCollection();

		public MapValueCollection() { }

		// Mock
		public int CompareTo(MapValueCollection? other)
		{
			if (other == null) return 1;
																	 
			return 0;
		}

		// Mock
		public int CompareTo(object? obj)
		{
			if (obj == null) return 1; 
			if (obj is MapValueCollection other)
				return CompareTo(other);
			return 1;
		}

		public override string ToString()
		{
			if (values.Count == 0)
			{
				return "{}";
			}
			else if (values.Count == 1)
			{
				var first = values[0];
				return $"{{({first.Key}, {first.Value})}}";
			} else if (values.Count == 2)
			{
				var first = values[0];
				var second = values[values.Count - 1];
				return $"{{({first.Key}, {first.Value}), ({second.Key}, {second.Value})}}";
			} else
			{
				var first = values[0];
				var last = values[values.Count - 1];
				return $"{{({first.Key}, {first.Value}) ... ({last.Key}, {last.Value})}}";
			}
		
		}

		private class OrderedCollection : KeyedCollection<MapValue, MapValue>
		{
			protected override MapValue GetKeyForItem(MapValue item)
			{
				return item;
			}
		}
	}
}
