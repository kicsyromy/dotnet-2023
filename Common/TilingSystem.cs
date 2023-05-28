using Mapster.Common.MemoryMappedTypes;

namespace Mapster.Common;

public static class TiligSystem
{
    // Retrieve the tile that a coordinate lies in
    public static int GetTile(Coordinate c)
    {
        var latitudeInt = (short)Math.Floor(c.Latitude);
        var longitudeInt = (short)Math.Floor(c.Longitude);

        // [.............32............]
        // [.....16......][.....16.....]
        //                [1...........]
        var result = latitudeInt << 16;
        // [.....16......][000000000000]
        result |= (ushort)longitudeInt;
        // [.....16......][000000000000] |
        //                [1...........]
        // -----------------------------
        // [.....16......][1...........]

        return result;
    }

    public static Coordinate LowerTileBound(int tileId)
    {
        // [.....16......][.....16.....] &
        // [0000000000000][111111111111]
        // -----------------------------
        // [0000000000000][.....16.....]
        var latitude = tileId >> 16;
        var longitude = (short)(tileId & 0x0000ffff);
        return new Coordinate(latitude, longitude);
    }

    // Retrieve all the tile ids that belong to a bounding box (defined by two coordinate pairs)
    public static int[] GetTilesForBoundingBox(double minLatitude, double minLongitude, double maxLatitude, double maxLongitude)
    {
        var result = new List<int>();

        var minLatitudeInt = (short)Math.Floor(minLatitude);
        var minLongitudeInt = (short)Math.Floor(minLongitude);
        var maxLatitudeInt = (short)Math.Floor(maxLatitude);
        var maxLongitudeInt = (short)Math.Floor(maxLongitude);

        for (var i = minLatitudeInt; i <= maxLatitudeInt; ++i)
        {
            for (var j = minLongitudeInt; j <= maxLongitudeInt; ++j)
            {
                var tileId = i << 16;
                tileId |= (ushort)j;
                result.Add(tileId);
            }
        }

        return result.ToArray();
    }
}
