using System.Collections;

namespace OSMDataParser;

using Tag = KeyValuePair<string, string>;

public abstract class AbstractTagList : IReadOnlyList<Tag>
{
    public abstract Tag this[int index] { get; }

    public abstract int Count { get; }

    public IEnumerator<Tag> GetEnumerator()
    {
        return new TagEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class TagEnumerator : IEnumerator<Tag>
{
    private bool _disposedValue = false;
    private Tag _currentTag = new Tag();
    private AbstractTagList _tagList;
    private int _tagCount = 0;
    private int _currentIndex = 0;

    public Tag Current => _currentTag;

    object IEnumerator.Current => Current;

    public TagEnumerator(AbstractTagList tagList)
    {
        _tagList = tagList;
        _tagCount = tagList.Count;
    }

    public bool MoveNext()
    {
        if (_currentIndex >= _tagCount)
            return false;

        _currentTag = _tagList[_currentIndex++];
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
