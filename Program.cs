// |---------||---------||---------|(maxLatitude,maxLongitude)
// |         ||         ||         |
// |         ||         ||         |
// |         ||         ||         |
// |---------||---------||---------|
// |---------||---------||---------|
// |         ||         ||         |
// |         ||         ||         |
// |         ||         ||         |
// |---------||---------||---------|
// (minLatitude,minLongitude)
// (23         ,100)

namespace Rendering
{
    public struct Coordinate
    {
        public double Latitude;
        public double Longitude;

        public Coordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    public enum GeometryType : byte
    {
        Polyline,
        Polygon,
    }

    public struct MapFeature
    {
        public long Id;
        public string Label;
        public Coordinate[] Coordinates;
        public GeometryType Type;
        public Dictionary<string, string> Properties;
    }

    class Program
    {
        // 22.3467534786,45.49835793487
        // 22.3228574377,45.98346587634
        // 22.8546756344,45.49523464890
        // 22-45

        Dictionary<int, List<MapFeature>> features = new Dictionary<int, List<MapFeature>>();

        // Retrieve the tile that a coordinate lies in
        public static int GetTile(Coordinate c)
        {
            var latitudeInt = (short)Math.Floor(c.Latitude);
            var longitudeInt = (short)Math.Floor(c.Longitude);

            // [.............32............]
            // [.....16......][.....16.....]
            //                [1...........]

            int result = (int)latitudeInt << 16;
            // [.....16......][000000000000]
            result |= (UInt16)longitudeInt;
            // [.....16......][000000000000] |
            //                [1...........]
            // -----------------------------
            // [.....16......][1...........]

            return result;
        }

        public static string DecodeTile(int tileId)
        {
            // [.....16......][.....16.....] &
            // [0000000000000][111111111111]
            // -----------------------------
            // [0000000000000][.....16.....]

            var latitude = tileId >> 16;
            var longitude = (short)(tileId & 0x0000ffff);
            return $"{latitude}-{longitude}";
        }

        // Retrieve all the tile ids that belong to a bounding box (defined by two coordinate pairs)
        public static int[] GetTilesForBoundingBox(double minLatitude, double minLongitude, double maxLatitude, double maxLongitude)
        {
            var result = new List<int>();

            var minLatitudeInt = (short)Math.Floor(minLatitude);
            var minLongitudeInt = (short)Math.Floor(minLongitude);
            var maxLatitudeInt = (short)Math.Floor(maxLatitude);
            var maxLongitudeInt = (short)Math.Floor(maxLongitude);

            for (short i = minLatitudeInt; i < maxLatitudeInt; ++i)
            {
                for (short j = minLongitudeInt; j < maxLongitudeInt; ++j)
                {
                    int tileId = (int)i << 16;
                    tileId |= (UInt16)j;
                    result.Add(tileId);
                }
            }

            return result.ToArray();
        }

        // Organize and split features into their respective tiles
        public static long OrganizeFeature(MapFeature feature, Dictionary<int, List<MapFeature>> storage, long guid)
        {
            Dictionary<int, MapFeature> splitByTile = new Dictionary<int, MapFeature>();
            foreach (var coord in feature.Coordinates)
            {
                var tileId = GetTile(coord);
                MapFeature newFeature = new MapFeature();
                newFeature.Properties = feature.Properties;
                newFeature.Label = feature.Label;
                newFeature.Id = guid;
                newFeature.Type = feature.Type;
                if (splitByTile.ContainsKey(tileId))
                {
                    newFeature.Coordinates = new Coordinate[splitByTile[tileId].Coordinates.Length + 1];
                    Array.Copy(splitByTile[tileId].Coordinates, newFeature.Coordinates, splitByTile[tileId].Coordinates.Length);
                    newFeature.Coordinates[splitByTile[tileId].Coordinates.Length] = coord;
                    splitByTile[tileId] = newFeature;
                }
                else
                {
                    newFeature.Coordinates = new Coordinate[1];
                    newFeature.Coordinates[0] = coord;
                    splitByTile[tileId] = newFeature;
                }
            }
            return guid + 1;
        }

        // This is the serialization format used for our binary representation of
        // map features

        // [tileId][offset1][tileId][offset2][tileId][offset3]...
        // offset1: [feature][feature][feature]...
        // offset2: [feature][feature][feature]...
        // offset3: [feature][feature][feature]...
        // .......

        // Writes a placeholder header into the file to reserve the neccesarry space for the real header
        public static void WriteDummyHeader(BinaryWriter bWriter, Dictionary<int, List<MapFeature>> data)
        {
            foreach (var entry in data)
            {
                bWriter.Write(entry.Key);
                bWriter.Write((long)0);
            }
        }

        // Serializez one feature to a binary representation and writes it out to a file
        public static void WriteFeatureToFile(BinaryWriter bWriter, MapFeature feature)
        {
            bWriter.Write(feature.Id);
            bWriter.Write(feature.Label);
            bWriter.Write((byte)feature.Type);
            bWriter.Write(feature.Coordinates.Length);
            foreach (var coord in feature.Coordinates)
            {
                bWriter.Write(coord.Latitude);
                bWriter.Write(coord.Longitude);
            }
            bWriter.Write(feature.Properties.Count);
            foreach (var entry in feature.Properties)
            {
                bWriter.Write(entry.Key);
                bWriter.Write(entry.Value);
            }
        }

        // Writes a list of features to a file
        public static void WriteToFile(string path, MapFeature[] features)
        {
            long globalId = 0;
            Dictionary<int, List<MapFeature>> organizedData = new Dictionary<int, List<MapFeature>>();
            Dictionary<int, long> offsets = new Dictionary<int, long>();
            foreach (var feature in features)
            {
                globalId = OrganizeFeature(feature, organizedData, globalId);
            }

            using (var fStream = new FileStream(path, FileMode.OpenOrCreate))
            using (var bWriter = new BinaryWriter(fStream))
            {
                bWriter.Write(organizedData.Count); // Number of entries in the header.
                WriteDummyHeader(bWriter, organizedData); // Allocates the space in the file for the header.
                foreach (var entry in organizedData)
                {
                    // Take note of the offset for this entry
                    offsets[entry.Key] = fStream.Position;
                    bWriter.Write(entry.Value.Count); // Number of features in the tile.
                    foreach (var feature in entry.Value)
                    {
                        WriteFeatureToFile(bWriter, feature);
                    }
                }
                bWriter.Flush();

                // Seek back to the beggining of the file and overwrite the dummy header with actual data
                fStream.Seek(sizeof(int), SeekOrigin.Begin);
                foreach (var entry in offsets)
                {
                    bWriter.Write(entry.Key);
                    bWriter.Write(entry.Value);
                }
            }
        }

        public static void Main()
        {
            var tileId = DecodeTile(GetTile(new Coordinate(22.3467534786, 45.49835793487)));
            var tilesInBBox = GetTilesForBoundingBox(33.81570572787154, -120.00531972269789, 42.69862059881901, -108.35981308058435)
                            .Select(tileId => DecodeTile(tileId))
                            .ToArray();

            Console.Write("[");
            foreach (var tile in tilesInBBox)
            {
                Console.Write($"{tile} ");
            }
            Console.WriteLine("]");
        }
    }
}
