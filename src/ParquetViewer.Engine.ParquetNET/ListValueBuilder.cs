using ParquetViewer.Engine.Types;
using System.Collections;

namespace ParquetViewer.Engine.ParquetNET;

public class ListValueBuilder : RepeatedValueBuilder<ListValue>
{
    private readonly IReadOnlyList<object> _values;
    private readonly Type _type;

    public ListValueBuilder(FieldLevels levels, int[] repetitionLevels, int[] definitionLevels, IReadOnlyList<object> values, Type type)
        : base(parentRepetitionBoundary: 0, levels, repetitionLevels, definitionLevels)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(type);

        if (values.Count != definitionLevels.Length)
            throw new Exceptions.ParquetEngineException("List data and its levels must be the same length");

        _values = values;
        _type = type;
    }

    /// <summary>
    /// Reads a (possibly nested) list value
    /// </summary>
    protected override ListValue? Build(Range range)
    {
        var levels = Levels;

        LinkedArrayList root = new();
        var lastByDepth = new LinkedArrayList?[levels.MaxRepetitionLevel + 1]; //Use a 1-based (instead of 0-based) array for simplicity

        void AddPoint(LinkedArrayList listNode, int listDepth, int index)
        {
            lastByDepth[listDepth] = listNode;

            var definitionLevel = DefinitionLevels[index];
            if (levels.IsNull(definitionLevel, listDepth))
            {
                //This list is null
                listNode.IsNull = true;
                return;
            }

            if (levels.IsEmpty(definitionLevel, listDepth))
            {
                //This list is present but empty
                listNode.IsEmpty = true;
                return;
            }

            if (listDepth == levels.MaxRepetitionLevel)
            {
                //We've reached the innermost list, so this is the element value itself
                listNode.Add(_values[index]);
                return;
            }

            //Descend a level to the next nested list
            var childNode = new LinkedArrayList(listDepth + 1, listNode);
            listNode.Add(childNode);
            AddPoint(childNode, listDepth + 1, index);
        }

        //First data point is always added to the most nested list
        AddPoint(root, 1, range.Start.Value);

        for (var index = range.Start.Value + 1; index < range.End.Value; index++)
        {
            var repetitionLevel = RepetitionLevels[index];

            //repetitionLevel can never be larger than maxRepetitionLevel
            if (repetitionLevel == levels.MaxRepetitionLevel)
            {
                //We're still in the same (deepest) list, append to it
                lastByDepth[repetitionLevel]!.Add(_values[index]);
                continue;
            }

            //So, if we got here, we need to start a new sibling list at depth `repetitionLevel + 1`
            var newDepth = repetitionLevel + 1;
            var previousList = lastByDepth[newDepth] //We should already have a ListValue at this depth by now thanks to adding the first data point
                ?? throw new Exceptions.ParquetEngineException("Failed to construct nested list structure. Your file might be malformed.");

            //Start the sibling list and recurse
            var newList = previousList.NextList();
            AddPoint(newList, newDepth, index);
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
                    convertedArray.Add(ConstructListValues(childArray) ?? (object)DBNull.Value);
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