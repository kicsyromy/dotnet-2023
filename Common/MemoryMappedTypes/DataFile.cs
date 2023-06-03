using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mapster.Common.MemoryMappedTypes;

/// <summary>
///     Action to be called when iterating over <see cref="MapFeature" /> in a given bounding box via a call to
///     <see cref="DataFile.ForeachFeature" />
/// </summary>
/// <param name="feature">The current <see cref="MapFeature" />.</param>
/// <param name="label">The label of the feature, <see cref="string.Empty" /> if not available.</param>
/// <param name="coordinates">The coordinates of the <see cref="MapFeature" />.</param>
/// <returns></returns>
public delegate bool MapFeatureDelegate(MapFeatureData featureData);

/// <summary>
///     Aggregation of all the data needed to render a map feature
/// </summary>
public readonly ref struct MapFeatureData
{
    public long Id { get; init; }
    public GeometryType Type { get; init; }
    public ReadOnlySpan<char> Label { get; init; }
    public ReadOnlySpan<Coordinate> Coordinates { get; init; }
    public FeatureProperty Properties { get; init; }
}


/// <summary>
///     Represents a file with map data organized in the following format:<br />
///     <see cref="FileHeader" /><br />
///     Array of <see cref="TileHeaderEntry" /> with <see cref="FileHeader.TileCount" /> records<br />
///     Array of tiles, each tile organized:<br />
///     <see cref="TileBlockHeader" /><br />
///     Array of <see cref="MapFeature" /> with <see cref="TileBlockHeader.FeaturesCount" /> at offset
///     <see cref="TileHeaderEntry.OffsetInBytes" /> + size of <see cref="TileBlockHeader" /> in bytes.<br />
///     Array of <see cref="Coordinate" /> with <see cref="TileBlockHeader.CoordinatesCount" /> at offset
///     <see cref="TileBlockHeader.CharactersOffsetInBytes" />.<br />
///     Array of <see cref="StringEntry" /> with <see cref="TileBlockHeader.StringCount" /> at offset
///     <see cref="TileBlockHeader.StringsOffsetInBytes" />.<br />
///     Array of <see cref="char" /> with <see cref="TileBlockHeader.CharactersCount" /> at offset
///     <see cref="TileBlockHeader.CharactersOffsetInBytes" />.<br />
/// </summary>
public unsafe class DataFile : IDisposable
{
    private readonly FileHeader* _fileHeader;
    private readonly MemoryMappedViewAccessor _mma;
    private readonly MemoryMappedFile _mmf;

    private readonly byte* _ptr;
    private readonly int CoordinateSizeInBytes = Marshal.SizeOf<Coordinate>();
    private readonly int FileHeaderSizeInBytes = Marshal.SizeOf<FileHeader>();
    private readonly int MapFeatureSizeInBytes = Marshal.SizeOf<MapFeature>();
    private readonly int StringEntrySizeInBytes = Marshal.SizeOf<StringEntry>();
    private readonly int TileBlockHeaderSizeInBytes = Marshal.SizeOf<TileBlockHeader>();
    private readonly int TileHeaderEntrySizeInBytes = Marshal.SizeOf<TileHeaderEntry>();

    private bool _disposedValue;

    public DataFile(string path)
    {
        _mmf = MemoryMappedFile.CreateFromFile(path);
        _mma = _mmf.CreateViewAccessor();
        _mma.SafeMemoryMappedViewHandle.AcquirePointer(ref _ptr);
        _fileHeader = (FileHeader*)_ptr;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _mma?.SafeMemoryMappedViewHandle.ReleasePointer();
                _mma?.Dispose();
                _mmf?.Dispose();
            }

            _disposedValue = true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private TileHeaderEntry* GetNthTileHeader(int i)
    {
        return (TileHeaderEntry*)(_ptr + i * TileHeaderEntrySizeInBytes + FileHeaderSizeInBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private (TileBlockHeader? Tile, ulong TileOffset) GetTile(int tileId)
    {
        ulong tileOffset = 0;
        for (var i = 0; i < _fileHeader->TileCount; ++i)
        {
            var tileHeaderEntry = GetNthTileHeader(i);
            if (tileHeaderEntry->ID == tileId)
            {
                tileOffset = tileHeaderEntry->OffsetInBytes;
                return (*(TileBlockHeader*)(_ptr + tileOffset), tileOffset);
            }
        }

        return (null, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private MapFeature* GetFeature(int i, ulong offset)
    {
        return (MapFeature*)(_ptr + offset + TileBlockHeaderSizeInBytes + i * MapFeatureSizeInBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private ReadOnlySpan<Coordinate> GetCoordinates(ulong coordinateOffset, int ithCoordinate, int coordinateCount)
    {
        return new ReadOnlySpan<Coordinate>(_ptr + coordinateOffset + ithCoordinate * CoordinateSizeInBytes, coordinateCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void GetString(ulong stringsOffset, ulong charsOffset, int i, out ReadOnlySpan<char> value)
    {
        var stringEntry = (StringEntry*)(_ptr + stringsOffset + i * StringEntrySizeInBytes);
        value = new ReadOnlySpan<char>(_ptr + charsOffset + stringEntry->Offset * 2, stringEntry->Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void GetProperty(ulong stringsOffset, ulong charsOffset, int i, out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)
    {
        if (i % 2 != 0)
        {
            throw new ArgumentException("Properties are key-value pairs and start at even indices in the string list (i.e. i % 2 == 0)");
        }

        GetString(stringsOffset, charsOffset, i, out key);
        GetString(stringsOffset, charsOffset, i + 1, out value);
    }
    private FeatureProperty ParseProperties(Dictionary<string, string> properties)
{
    FeatureProperty result = FeatureProperty.None;

    foreach (var property in properties)
    {
        string key = property.Key;
        string value = property.Value;

        if (key == "highway" && MapFeature.HighwayTypes.Any(v => value.StartsWith(v)))
            result |= FeatureProperty.Highway;

        if (key.StartsWith("water"))
            result |= FeatureProperty.Water;

        if (key.StartsWith("border"))
            result |= FeatureProperty.Border;

            if (key.StartsWith("place"))
            {
                if (value.StartsWith("city") || value.StartsWith("town") || value.StartsWith("locality") || value.StartsWith("hamlet"))
                    result |= FeatureProperty.PopulatedPlaceTrue;
                else
                    result |= FeatureProperty.PopulatedPlaceFalse;
            }


            if (key.StartsWith("railway"))
            result |= FeatureProperty.Railway;

        if (key.StartsWith("natural") && (value.StartsWith("grassland") || value.StartsWith("fell")  || value.StartsWith("heath")  || value.StartsWith("moor")  || value.StartsWith("scrub")  || value.StartsWith("wetland") ))
            result |= FeatureProperty.PlainNatural;

        if (key.StartsWith("natural") && value.StartsWith("water"))
            result |= FeatureProperty.WaterNatural;
        
        if (key.StartsWith("natural") && (value.StartsWith("wood") || value.StartsWith("tree_row") ))
            result |= FeatureProperty.ForestNatural;
        
        if (key.StartsWith("natural") && (value.StartsWith("bare_rock") || value.StartsWith("rock")  || value.StartsWith("scree") ))
            result |= FeatureProperty.MountainNatural;

         if (key.StartsWith("natural") && (value.StartsWith("beach") || value.StartsWith("sand")))
            result |= FeatureProperty.DesertNatural;

        if (key.StartsWith("boundary") && value.StartsWith("forest"))
            result |= FeatureProperty.ForestBoundary;

        if (key.StartsWith("landuse") && (value.StartsWith("forest") || value.StartsWith("orchard")))
            result |= FeatureProperty.ForestLanduse;

        if (key.StartsWith("landuse") && (value.StartsWith("residential") || value.StartsWith("cemetery") || value.StartsWith("industrial") || value.StartsWith("commercial") ||
                                          value.StartsWith("square") || value.StartsWith("construction") || value.StartsWith("military") || value.StartsWith("quarry") ||
                                          value.StartsWith("brownfield")))
            result |= FeatureProperty.ResidentialLanduse;

        if (key.StartsWith("landuse") && (value.StartsWith("farm") || value.StartsWith("meadow") || value.StartsWith("grass") || value.StartsWith("greenfield") ||
                                          value.StartsWith("recreation_ground") || value.StartsWith("winter_sports") || value.StartsWith("allotments")))
            result |= FeatureProperty.PlainLanduse;

        if (key.StartsWith("landuse") && (value.StartsWith("reservoir") || value.StartsWith("basin")))
            result |= FeatureProperty.ReservoirLanduse;

        if (key.StartsWith("building"))
            result |= FeatureProperty.Building;

        if (key.StartsWith("leisure"))
            result |= FeatureProperty.Leisure;

        if (key.StartsWith("amenity"))
            result |= FeatureProperty.Amenity;
    }

    return result;
}


    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void ForeachFeature(BoundingBox b, MapFeatureDelegate? action)
    {
        if (action == null)
        {
            return;
        }

        var tiles = TiligSystem.GetTilesForBoundingBox(b.MinLat, b.MinLon, b.MaxLat, b.MaxLon);
        for (var i = 0; i < tiles.Length; ++i)
        {
            var header = GetTile(tiles[i]);
            if (header.Tile == null)
            {
                continue;
            }
            for (var j = 0; j < header.Tile.Value.FeaturesCount; ++j)
            {
                var feature = GetFeature(j, header.TileOffset);
                var coordinates = GetCoordinates(header.Tile.Value.CoordinatesOffsetInBytes, feature->CoordinateOffset, feature->CoordinateCount);
                var isFeatureInBBox = false;

                for (var k = 0; k < coordinates.Length; ++k)
                {
                    if (b.Contains(coordinates[k]))
                    {
                        isFeatureInBBox = true;
                        break;
                    }
                }

                var label = ReadOnlySpan<char>.Empty;
                if (feature->LabelOffset >= 0)
                {
                    GetString(header.Tile.Value.StringsOffsetInBytes, header.Tile.Value.CharactersOffsetInBytes, feature->LabelOffset, out label);
                }

                if (isFeatureInBBox)
                {
                    var properties = new Dictionary<string, string>(feature->PropertyCount);
                    for (var p = 0; p < feature->PropertyCount; ++p)
                    {
                        GetProperty(header.Tile.Value.StringsOffsetInBytes, header.Tile.Value.CharactersOffsetInBytes, p * 2 + feature->PropertiesOffset, out var key, out var value);
                        properties.Add(key.ToString(), value.ToString());
                    }

                    var parsedProperties = ParseProperties(properties);

                    if (!action(new MapFeatureData
                    {
                        Id = feature->Id,
                        Label = label,
                        Coordinates = coordinates,
                        Type = feature->GeometryType,
                        Properties = parsedProperties
                    }))
                    {
                        break;
                    }

                }
            }
        }
    }
}
