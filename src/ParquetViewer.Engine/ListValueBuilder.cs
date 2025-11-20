using ParquetViewer.Engine.Types;
using System.Collections;

namespace ParquetViewer.Engine
{
    internal class ListValueBuilder
    {
        private int[] _repetitionLevels;
        private int[] _definitionLevels;
        private IEnumerable<object> _data;
        private Type _type;

        public ListValueBuilder(int[] repetitionLevels, int[] definitionLevels, IEnumerable<object> data, Type type)
        {
            ArgumentNullException.ThrowIfNull(definitionLevels);
            ArgumentNullException.ThrowIfNull(repetitionLevels);
            ArgumentNullException.ThrowIfNull(data);
            ArgumentNullException.ThrowIfNull(type);

            _type = type;

            //We assume they all have the same length
            _definitionLevels = definitionLevels;
            _repetitionLevels = repetitionLevels;
            _data = data;
        }

        private IEnumerable<Range> GetRowRanges()
        {
            int startIndex = 0;
            int endIndex = 0;
            for (int i = 1; i < _repetitionLevels.Length; i++)
            {
                if (_repetitionLevels[i] == 0)
                {
                    endIndex = i;
                    yield return new(startIndex, endIndex);

                    startIndex = i;
                }
            }

            yield return new(startIndex, _repetitionLevels.Length);
        }

        /// <summary>
        /// Reads nested list values
        /// </summary>
        /// <returns>Enumerable of ListValue's. We need to return object to support DBNull.Value</returns>
        public IEnumerable<object> ReadRows(int skipRecords, int readRecords, int numberOfListParents, int currentDefinitionLevel, int maxDefinitionLevel, CancellationToken cancellationToken)
        {
            var ranges = GetRowRanges();

            var rowRangesToRead = ranges.Skip(skipRecords).Take(readRecords);
            foreach (var rowRange in rowRangesToRead)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var listValue = ReadListValue(rowRange, numberOfListParents, () =>
                {
                    //TODO: optimize to avoid skipping all rows every time
                    return _data.Skip(rowRange.Start.Value).Take(rowRange.End.Value - rowRange.Start.Value).ToArray();
                },
                (int index) =>
                {
                    return _definitionLevels.Length > index && _definitionLevels[index] == currentDefinitionLevel;
                });
                yield return listValue;
            }
        }

        private object ReadListValue(Range range, int numberOfListParents, Func<object[]> dataProvider, Func<int, bool> isEmptyProvider)
        {
            var rangeRepetition = _repetitionLevels.AsSpan(range);
            var rangeData = dataProvider.Invoke();

            if (rangeData.All(data => data == DBNull.Value))
            {
                if (isEmptyProvider(range.Start.Value))
                {
                    return new ListValue([], _type);
                }
                else
                {
                    return DBNull.Value;
                }
            }

            LinkedArrayList root = new();
            var node = root.GoDownToLevel(numberOfListParents);

            //First data point is always added to the most nested list
            node.Add(rangeData[0]);

            //Add everything else now
            for (var index = 1; index < rangeRepetition.Length; index++)
            {
                var repetitionLevel = rangeRepetition[index];
                var data = rangeData[index];

                if (repetitionLevel == numberOfListParents)
                {
                    //We're still in the same level, append to the current list
                    node.Add(data);
                    continue;
                }

                if (repetitionLevel == numberOfListParents - 1)
                {
                    node = node.NextList();
                    node.Add(data);
                    node.IsNull = data == DBNull.Value && !isEmptyProvider(range.Start.Value + index);
                    node.IsEmpty = data == DBNull.Value && isEmptyProvider(range.Start.Value + index);
                    continue;
                }

                var count = numberOfListParents;
                while (repetitionLevel < count - 1)
                {
                    node = node.Parent!;
                    count--;
                }

                node = node.NextList();
                node = node.GoDownToLevel(numberOfListParents);
                node.Add(data);
                node.IsNull = data == DBNull.Value && !isEmptyProvider(range.Start.Value + index);
                node.IsEmpty = data == DBNull.Value && isEmptyProvider(range.Start.Value + index);
            }

            return ConstructListValues(root);
        }


        private object ConstructListValues(LinkedArrayList array)
        {
            if (array.IsNull)
                return DBNull.Value;

            var hasChildArrays = false;
            var convertedArray = new ArrayList();

            if (!array.IsEmpty)
            {
                foreach (var data in array)
                {
                    if (data is LinkedArrayList childArray)
                    {
                        convertedArray.Add(ConstructListValues(childArray));
                        hasChildArrays = true;
                    }
                    else
                    {
                        convertedArray.Add(data);
                    }
                }
            }

            var type = hasChildArrays ? typeof(ListValue) : _type;
            return new ListValue(convertedArray, type);
        }

        private class LinkedArrayList : ArrayList
        {
            public LinkedArrayList? Parent { get; set; }
            public int Level { get; private set; }
            public bool IsNull { get; set; }
            public bool IsEmpty { get; set; }

            public LinkedArrayList Root
            {
                get
                {
                    var node = this;
                    while (node.Parent is not null)
                    {
                        node = node.Parent;
                    }
                    return node;
                }
            }

            public LinkedArrayList()
            {
                Level = 1;
                Parent = null;
            }

            private LinkedArrayList(int level, LinkedArrayList? parent = null)
            {
                Level = level;
                Parent = parent;
            }

            public LinkedArrayList GoDownToLevel(int level)
            {
                var node = this;
                while (node.Level < level)
                {
                    var childNode = new LinkedArrayList(node.Level + 1, node);
                    node.Add(childNode);
                    node = childNode;
                }
                return node;
            }

            public LinkedArrayList NextList()
            {
                if (this.Parent is null)
                    throw new InvalidOperationException();

                var nextList = new LinkedArrayList(this.Level, this.Parent);
                this.Parent.Add(nextList);
                return nextList;
            }
        }
    }
}
