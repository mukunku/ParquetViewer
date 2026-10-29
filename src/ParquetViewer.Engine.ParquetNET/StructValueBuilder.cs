using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;

namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// Builds <see cref="StructValue"/>s by reading each of the struct's fields with its own builder.
/// </summary>
internal sealed class StructValueBuilder : FieldValueBuilder
{
    private readonly int _definitionLevel;
    private readonly FieldValueBuilder[] _fields;
    private readonly LeafCursor[] _leaves;

    //Shared by every struct this builder creates, so each instance only costs its values
    private readonly string[] _fieldNames;

    /// <param name="definitionLevel">The definition level at which the struct exists. Anything lower means it's null.</param>
    public StructValueBuilder(int definitionLevel, IReadOnlyList<string> fieldNames, IReadOnlyList<FieldValueBuilder> fields)
    {
        ArgumentNullException.ThrowIfNull(fieldNames);
        ArgumentNullException.ThrowIfNull(fields);

        if (fields.Count == 0)
            throw new ParquetEngineException("A struct must have at least one field");
        if (fieldNames.Count != fields.Count)
            throw new ArgumentException("Every struct field needs a name", nameof(fieldNames));

        _definitionLevel = definitionLevel;
        _fields = fields.ToArray();
        _leaves = fields.SelectMany(field => field.Leaves).ToArray();
        _fieldNames = fieldNames.ToArray();
    }

    public override Type ValueType => typeof(StructValue);

    public override LeafCursor FirstLeaf => _fields[0].FirstLeaf;

    public override IReadOnlyList<LeafCursor> Leaves => _leaves;

    public override object Read()
    {
        //If the first field says the struct is null, all of should as well.
        //So we only need to check the first leaf.
        if (FirstLeaf.DefinitionLevel < _definitionLevel)
        {
            SkipEmptyInstance();
            return DBNull.Value;
        }

        var values = new object[_fields.Length];
        for (var i = 0; i < _fields.Length; i++)
        {
            values[i] = _fields[i].Read();
        }

        return new StructValue(_fieldNames, values);
    }
}
