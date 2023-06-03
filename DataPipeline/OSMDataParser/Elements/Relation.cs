namespace OSMDataParser.Elements;

public class Relation : AbstractElementInternal
{
    public override long Id => throw new NotImplementedException();

    public override AbstractTagList Tags => throw new NotImplementedException();

    internal override void SetStringTable(StringTable stringTable)
    {
        throw new NotImplementedException();
    }
}
