using ParquetViewer.Engine.Types;
using System.Collections;

namespace ParquetViewer.Engine.ParquetNET;

public class ListValueBuilder
{
    private readonly int[] _repetitionLevels;
    private readonly int[] _definitionLevels;
    private readonly IEnumerable<object> _data;
    private readonly Type _type;

    public ListValueBuilder(int[] repetitionLevels, int[] definitionLevels, IEnumerable<object> data, Type type)
    {
        ArgumentNullException.ThrowIfNull(definitionLevels);
        ArgumentNullException.ThrowIfNull(repetitionLevels);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(type);

        if (type == typeof(byte[]))
            _type = typeof(ByteArrayValue);
        else
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
    /// <returns>Enumerable of ListValue's, possibly null.</returns>
    /// <remarks>Callers will need to convert nulls to DBNull</remarks>
    public IEnumerable<ListValue?> ReadRows(int skipRecords, int readRecords, int numberOfListParents,
        IReadOnlyList<int> listDefinitionLevels, CancellationToken cancellationToken)
    {
        var ranges = GetRowRanges();

        var rowRangesToRead = ranges.Skip(skipRecords).Take(readRecords);
        foreach (var rowRange in rowRangesToRead)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var listValue = ReadListValue(rowRange, numberOfListParents, listDefinitionLevels, () =>
            {
                //TODO: optimize to avoid skipping all rows every time
                return _data
                .Select(data => data is byte[] bytes ? new ByteArrayValue(bytes) : data) //Need to handle byte array type separately
                .Skip(rowRange.Start.Value)
                .Take(rowRange.End.Value - rowRange.Start.Value)
                .ToArray();
            });
            yield return listValue;
        }
    }

    private ListValue? ReadListValue(Range range, int numberOfListParents, IReadOnlyList<int> listDefinitionLevels, Func<object[]> dataProvider)
    {
        var rangeRepetition = _repetitionLevels.AsSpan(range);
        var rangeData = dataProvider.Invoke();

        var definitionStartLevel = _definitionLevels.Length > range.Start.Value ? _definitionLevels[range.Start.Value] : 0;

        LinkedArrayList root = new();
        var lastByDepth = new LinkedArrayList?[numberOfListParents + 1];

        void AddPoint(LinkedArrayList listNode, int listDepth, object data, int definitionLevel)
        {
            lastByDepth[listDepth] = listNode;

            if (listDefinitionLevels.Count >= listDepth) //This should be true in almost all cases. Maybe two-tier lists and a few other exceptions?
            {
                if (definitionLevel < listDefinitionLevels[listDepth - 1])
                {
                    //This list is null
                    listNode.IsNull = true;
                    return;
                }

                if (definitionLevel == listDefinitionLevels[listDepth - 1])
                {
                    //This list is present but empty
                    listNode.IsEmpty = true;
                    return;
                }
            }

            if (listDepth == numberOfListParents)
            {
                //We've reached the innermost list, so this is the element value itself
                listNode.Add(data);
                return;
            }

            //Descend to the next nested list
            var childNode = new LinkedArrayList(listDepth + 1, listNode);
            listNode.Add(childNode);
            AddPoint(childNode, listDepth + 1, data, definitionLevel);
        }

        //First data point is always added to the most nested list
        AddPoint(root, 1, rangeData[0], definitionStartLevel);

        for (var index = 1; index < rangeRepetition.Length; index++)
        {
            var repetitionLevel = rangeRepetition[index];
            var data = rangeData[index];
            var definitionLevel = _definitionLevels.Length > range.Start.Value + index ? _definitionLevels[range.Start.Value + index] : 0;

            //repetitionLevel can never be larger than numberOfListParents
            if (repetitionLevel == numberOfListParents)
            {
                //We're still in the same (deepest) list, append to it
                lastByDepth[repetitionLevel]!.Add(data);
                continue;
            }

            //So, if we got here, we need to start a new sibling list at depth `repetitionLevel + 1`
            var newDepth = repetitionLevel + 1;
            var previousList = lastByDepth[newDepth] //We should already have a ListValue at this depth by now thanks to adding the first data point
                ?? throw new Exceptions.ParquetEngineException("Failed to construct nested list structure. Your file might be malformed.");

            //Start the sibling list and recurse
            var newList = previousList.NextList();
            AddPoint(newList, newDepth, data, definitionLevel);
        }

        return ConstructListValues(root);
    }

    private ListValue? ConstructListValues(LinkedArrayList array)
    {
        if (array.IsNull)
            return null;

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

        public LinkedArrayList()
        {
            Level = 1;
            Parent = null;
        }

        public LinkedArrayList(int level, LinkedArrayList? parent = null)
        {
            Level = level;
            Parent = parent;
        }

        public LinkedArrayList NextList()
        {
            if (Parent is null)
                throw new InvalidOperationException();

            var nextList = new LinkedArrayList(Level, Parent);
            Parent.Add(nextList);
            return nextList;
        }
    }
}