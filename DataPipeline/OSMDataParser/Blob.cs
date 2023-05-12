using System.IO.Compression;
using System.Text.Json.Serialization;

using Protobuf = Google.Protobuf;

namespace OSMDataParser;

public enum BlobType
{
    Unknown,
    Header,
    Primitive
}

public struct Feature
{
    public const int Unknown = 0;
    public const int OsmSchemaV06 = 1;
    public const int DenseNodes = 2;
    public const int HistoricalInformation = 3;

    public bool IsDefined => Value != Unknown;
    public int Value { get; }
    public string? Name { get; }

    public Feature(int value = Unknown, string? name = null)
    {
        Value = value;
        Name = name;
    }
}

public class Blob
{
    public BlobType Type { get; }
    public bool IsCompressed { get; }
    [JsonIgnore]
    public ReadOnlyMemory<Byte> Content { get; }

    public Blob(BlobType type = BlobType.Unknown, bool isCompressed = false, ReadOnlyMemory<Byte> content = new ReadOnlyMemory<byte>())
    {
        Type = type;
        IsCompressed = isCompressed;
        Content = content;
    }

    public HeaderBlock ToHeaderBlock()
    {
        return new HeaderBlock(this);
    }

    public PrimitiveBlock ToPrimitiveBlock()
    {
        return new PrimitiveBlock(this);
    }
}

class Detail
{
    internal static T DeserializeContent<T>(Blob blob) where T : Protobuf.IMessage<T>, new()
    {
        var data = blob.Content.Span;

        if (!blob.IsCompressed)
        {
            return (new Protobuf.MessageParser<T>(() => new T())).ParseFrom(data);
        }
        else
        {
            unsafe
            {
                fixed (byte* buffer = &data[0])
                {
                    using (var stream = new UnmanagedMemoryStream(buffer, data.Length))
                    using (var zlibStream = new ZLibStream(stream, CompressionMode.Decompress))
                    {
                        return (new Protobuf.MessageParser<T>(() => new T())).ParseFrom(zlibStream);
                    }
                }
            }
        }
    }
}
