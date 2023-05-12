namespace OSMDataParser;

public abstract class AbstractElement
{
    public abstract long Id { get; }
    public abstract AbstractTagList Tags { get; }
}

public abstract class AbstractElementInternal : AbstractElement
{
    internal abstract void SetStringTable(StringTable stringTable);
}
