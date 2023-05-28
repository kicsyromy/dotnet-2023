namespace OSMDataParser;

internal class DenseNodes
{
    private OSMPBF.DenseNodes _osmDenseNodes;
    private PrimitiveBlock _primitiveBlock;

    private int _nextTagIndex;
    private int _nextNodeIndex;
    private long _lastNodeIdDecoded;
    private long _lastNodeLatDecoded;
    private long _lastNodeLonDecoded;

    public long Id => 0;

    public int Count => _osmDenseNodes.Id.Count;

    public DenseNodes(OSMPBF.DenseNodes osmDenseNodes, PrimitiveBlock primitiveBlock)
    {
        _osmDenseNodes = osmDenseNodes;
        _primitiveBlock = primitiveBlock;

        _nextTagIndex = 0;
        _nextNodeIndex = 0;
        _lastNodeIdDecoded = 0;
        _lastNodeLatDecoded = 0;
        _lastNodeLonDecoded = 0;
    }

    public Elements.DenseNode GetNextNode()
    {
        var newNode = new Elements.DenseNode(_osmDenseNodes, _nextNodeIndex, _nextTagIndex, _lastNodeIdDecoded, _lastNodeLatDecoded, _lastNodeLonDecoded, _primitiveBlock);

        // Tags come in pairs so multiply the tag count of the node by two before advancing it by one
        _nextTagIndex += (newNode.Tags.Count * 2) + 1;

        _lastNodeIdDecoded = newNode.Id;
        _lastNodeLatDecoded += _osmDenseNodes.Lat[_nextNodeIndex];
        _lastNodeLonDecoded += _osmDenseNodes.Lon[_nextNodeIndex];

        ++_nextNodeIndex;

        return newNode;
    }
}

