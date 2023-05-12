using System.Collections;

namespace OSMDataParser;

public class PrimitiveBlock : IReadOnlyList<PrimitiveGroup>
{
    private OSMPBF.PrimitiveBlock _osmPrimitiveBlock;
    private StringTable _stringTable;

    // Granularity, units of nanodegrees, used to store coordinates in this block
    public int Granularity => _osmPrimitiveBlock.Granularity;

    // Offset value between the output coordinates coordinates and the granularity grid, in units of nanodegrees.
    public int DateGranularity => _osmPrimitiveBlock.DateGranularity;
    public long OffsetLatitude => _osmPrimitiveBlock.LatOffset;

    // Granularity of dates, normally represented in units of milliseconds since the 1970 epoch.
    public long OffsetLongitude => _osmPrimitiveBlock.LonOffset;

    public PrimitiveGroup this[int index]
    {
        get
        {
            var primitiveGroup = _osmPrimitiveBlock.Primitivegroup[index];
            return new PrimitiveGroup(this, primitiveGroup, _stringTable);
        }
    }

    public int Count => _osmPrimitiveBlock.Primitivegroup.Count;

    public PrimitiveBlock(Blob blob)
    {
        _osmPrimitiveBlock = Detail.DeserializeContent<OSMPBF.PrimitiveBlock>(blob);
        _stringTable = new StringTable(_osmPrimitiveBlock.Stringtable.S);
    }

    public IEnumerator<PrimitiveGroup> GetEnumerator()
    {
        return new PrimitiveGroupEnumerator(this, _osmPrimitiveBlock.Primitivegroup.Count);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class PrimitiveGroupEnumerator : IEnumerator<PrimitiveGroup>
{
    private bool _disposedValue = false;
    private PrimitiveGroup _currentPrimitiveGroup;
    private PrimitiveBlock _primitiveBlock;
    private int _groupCount = 0;
    private int _currentIndex = 0;

    public PrimitiveGroup Current => _currentPrimitiveGroup;

    object IEnumerator.Current => Current;

    public PrimitiveGroupEnumerator(PrimitiveBlock primitiveBlock, int primitiveGroupCount)
    {
        _primitiveBlock = primitiveBlock;
        _groupCount = primitiveGroupCount;
        _currentPrimitiveGroup = new PrimitiveGroup(primitiveBlock);
    }

    public bool MoveNext()
    {
        if (_currentIndex >= _groupCount)
            return false;

        _currentPrimitiveGroup = _primitiveBlock[_currentIndex++];
        return true;
    }

    public void Reset()
    {
        _currentIndex = 0;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
