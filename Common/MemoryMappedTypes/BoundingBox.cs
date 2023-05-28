namespace Mapster.Common.MemoryMappedTypes
{
    public struct BoundingBox
    {
        public double MinLat;
        public double MaxLat;
        public double MinLon;
        public double MaxLon;

        public BoundingBox(Coordinate min, Coordinate max)
        {
            if(min.Longitude > max.Longitude ||
               min.Latitude > max.Latitude)
            {
                throw new ArgumentException("Minimum coordinate is greater than maximum coordinate");
            }
            MinLat = min.Latitude;
            MaxLat = max.Latitude;
            MinLon = min.Longitude;
            MaxLon = max.Longitude;
        }

        public BoundingBox(Coordinate a, Coordinate b, Coordinate c, Coordinate d)
        {
            MinLat = Math.Min(a.Latitude, Math.Min(b.Latitude, Math.Min(c.Latitude, d.Latitude)));
            MinLon = Math.Min(a.Longitude, Math.Min(b.Longitude, Math.Min(c.Longitude, d.Longitude)));
            MaxLat = Math.Max(a.Latitude, Math.Max(b.Latitude, Math.Max(c.Latitude, d.Latitude)));
            MaxLon = Math.Max(a.Longitude, Math.Max(b.Longitude, Math.Max(c.Longitude, d.Longitude)));
        }

        public bool Contains(Coordinate c)
        {
            return (MinLat <= c.Latitude && MinLon <= c.Longitude &&
               MaxLat >= c.Latitude && MaxLon >= c.Longitude);
        }
    }
}
