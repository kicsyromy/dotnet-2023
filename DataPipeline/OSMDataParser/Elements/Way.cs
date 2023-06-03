using System.Text.Json.Serialization;

namespace OSMDataParser.Elements;

public class Way : AbstractElementInternal
{
    private OSMPBF.Way _osmWay;
    private TagList? _tags;

    public override long Id => _osmWay.Id;
    [JsonIgnore]
    public override AbstractTagList Tags => _tags == null ? throw new InvalidDataException() : _tags;

    public long[] NodeIds { get; }

    public Way(OSMPBF.Way way)
    {
        _osmWay = way;
        NodeIds = new long[_osmWay.Refs.Count];

        // Delta coded node ids
        NodeIds[0] = _osmWay.Refs[0];
        for (var i = 1; i < NodeIds.Length; ++i)
        {
            NodeIds[i] = NodeIds[i - 1] + _osmWay.Refs[i];
        }
    }

    internal override void SetStringTable(StringTable stringTable)
    {
        _tags = new TagList(_osmWay.Keys, _osmWay.Vals, stringTable);
    }
}
