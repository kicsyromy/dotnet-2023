namespace OSMDataParser.Elements;

public class Unknown : AbstractElementInternal
{
    public override long Id => throw new InvalidOperationException();

    public override AbstractTagList Tags => throw new InvalidOperationException();

    internal override void SetStringTable(StringTable stringTable)
    {
        throw new NotImplementedException();
    }
}

