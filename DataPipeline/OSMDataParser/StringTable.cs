using System.Collections;
using Protobuf = Google.Protobuf;

namespace OSMDataParser;

internal class StringTable : IReadOnlyList<string>
{
    private IList<Protobuf.ByteString> _stringTable;

    public StringTable(IList<Protobuf.ByteString>? stringTable = null)
    {
        _stringTable = stringTable == null ? new Protobuf.Collections.RepeatedField<Protobuf.ByteString>() : stringTable;
    }

    public string this[int index]
    {
        get
        {
            var bytes = _stringTable[index].Span;
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }

    public int Count => _stringTable.Count;

    public IEnumerator<string> GetEnumerator()
    {
        return new StringTableEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

internal class StringTableEnumerator : IEnumerator<string>
{
    private bool _disposedValue = false;
    private string _currentValue = string.Empty;
    private StringTable _stringTable;
    private int _count = 0;
    private int _currentIndex = 0;

    public string Current => _currentValue;

    object IEnumerator.Current => Current;

    public StringTableEnumerator(StringTable stringTable)
    {
        _stringTable = stringTable;
        _count = stringTable.Count;
    }

    public bool MoveNext()
    {
        if (_currentIndex >= _count)
            return false;

        _currentValue = _stringTable[_currentIndex++];
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
