namespace TileRenderer;

public struct MapFeature
{
    public enum GeometryType : byte
    {
        Polyline,
        Polygon
    }

    public long Id { get; set; }
    public string Label { get; set; }
    public Coordinate[] Coordinates { get; set; }
    public GeometryType Type { get; set; }
}