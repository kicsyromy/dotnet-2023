using System.Runtime.InteropServices;

namespace Mapster.Common.MemoryMappedTypes;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct FileHeader
{
    [FieldOffset(0)] public long Version;
    [FieldOffset(8)] public int TileCount;
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct TileHeaderEntry
{
    [FieldOffset(0)] public int ID;
    [FieldOffset(4)] public ulong OffsetInBytes;
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct TileBlockHeader
{
    /// <summary>
    ///     Number of renderable features in the tile.
    /// </summary>
    [FieldOffset(0)] public int FeaturesCount;

    /// <summary>
    ///     Number of coordinates used for the features in the tile.
    /// </summary>
    [FieldOffset(4)] public int CoordinatesCount;

    /// <summary>
    ///     Number of strings used for the features in the tile.
    /// </summary>
    [FieldOffset(8)] public int StringCount;

    /// <summary>
    ///     Number of characters used by the strings in the tile.
    /// </summary>
    [FieldOffset(12)] public int CharactersCount;

    [FieldOffset(16)] public ulong CoordinatesOffsetInBytes;
    [FieldOffset(24)] public ulong StringsOffsetInBytes;
    [FieldOffset(32)] public ulong CharactersOffsetInBytes;
}

/// <summary>
///     References a string in a large character array.
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct StringEntry
{
    [FieldOffset(0)] public int Offset;
    [FieldOffset(4)] public int Length;
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct Coordinate
{
    [FieldOffset(0)] public double Latitude;
    [FieldOffset(8)] public double Longitude;

    public Coordinate()
    {
        Latitude = 0;
        Longitude = 0;
    }

    public Coordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public bool Equals(Coordinate other)
    {
        return Math.Abs(Latitude - other.Latitude) < double.Epsilon &&
               Math.Abs(Longitude - other.Longitude) < double.Epsilon;
    }

    public override bool Equals(object? obj)
    {
        return obj is Coordinate other && Equals(other);
    }

    public static bool operator ==(Coordinate self, Coordinate other)
    {
        return self.Equals(other);
    }

    public static bool operator !=(Coordinate self, Coordinate other)
    {
        return !(self == other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Latitude, Longitude);
    }
}

public enum GeometryType : byte
{
    Polyline,
    Polygon,
    Point
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct PropertyEntryList
{
    [FieldOffset(0)] public int Count;
    [FieldOffset(4)] public ulong OffsetInBytes;
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct MapFeature
{
    // https://wiki.openstreetmap.org/wiki/Key:highway
    public static string[] HighwayTypes =
    {
        "motorway", "trunk", "primary", "secondary", "tertiary", "unclassified", "residential", "road"
    };

    [FieldOffset(0)] public long Id;
    [FieldOffset(8)] public int LabelOffset;
    [FieldOffset(12)] public GeometryType GeometryType;
    [FieldOffset(13)] public int CoordinateOffset;
    [FieldOffset(17)] public int CoordinateCount;
    [FieldOffset(21)] public int PropertiesOffset;
    [FieldOffset(25)] public int PropertyCount;
}
