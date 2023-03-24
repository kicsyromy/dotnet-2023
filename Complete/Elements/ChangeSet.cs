
namespace OSMDataParser.Elements;

public class ChangeSet : AbstractElementInternal
{
    public override long Id { get; }

    public override AbstractTagList Tags => throw new NotImplementedException();

    internal override void SetStringTable(StringTable stringTable)
    {
        throw new NotImplementedException();
    }
}
