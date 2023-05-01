namespace TileRenderer;

public struct Coordinate : IEquatable<Coordinate>
{
    public double Latitude { get; set; }
    public double Longitude { get; set;  }

    public Coordinate(double longitude, double latitude)
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