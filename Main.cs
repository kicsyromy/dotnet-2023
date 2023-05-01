using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TileRenderer;

internal class Program
{
    private struct Pixel
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Pixel(Coordinate c)
        {
            X = (float)MercatorProjection.lonToX(c.Longitude);
            Y = (float)MercatorProjection.latToY(c.Latitude);
        }
    }

    private static void Render(MapFeature feature, int width, int height)
    {
        var minX = float.MaxValue;
        var maxX = float.MinValue;
        var minY = float.MaxValue;
        var maxY = float.MinValue;
        
        var shape = new Pixel[feature.Coordinates.Length];
        for (int i = 0; i < shape.Length; ++i)
        {
            var c = feature.Coordinates[i];
            var pixel =  new Pixel(c);
            minX = Math.Min(minX, pixel.X);
            minY = Math.Min(minY, pixel.Y);
            
            shape[i] = pixel;
        }
        
        for (int i = 0; i < shape.Length; ++i)
        {
            shape[i].X -= minX;
            shape[i].Y -= minY;
            
            maxX = Math.Max(maxX, shape[i].X);
            maxY = Math.Max(maxY, shape[i].Y);
        }

        var scaleX = width / maxX;
        var scaleY = height / maxY;
        var scale = Math.Min(scaleX, scaleY);

        var canvas = new Image<Rgba32>(width, height);
        var points = shape.Select(p => new PointF(p.X * scale, height - p.Y * scale)).ToArray();
        
        canvas.Mutate(context =>
        {
            var pen = new Pen(Color.White, 2.0f);
            context.DrawLines(pen, points);
        });
        
        canvas.SaveAsPng(@"output.png");
    }

    private static void Main()
    {
        var feature = new MapFeature
        {
            Id = 1,
            Label = "A Timisoara street",
            Type = MapFeature.GeometryType.Polyline,
            Coordinates = new[]
            {
                new Coordinate(
                    21.202532994025574,
                    45.749679315495456
                ),
                new Coordinate(
                    21.207764168450808,
                    45.750388359075856
                ),
                new Coordinate(
                    21.21596845639789,
                    45.751780159168874
                ),
                new Coordinate(
                    21.21578028465646,
                    45.75275177262395
                ),
                new Coordinate(
                    21.216909315108126,
                    45.756585541738616
                ),
                new Coordinate(
                    21.209420079779505,
                    45.75768835804661
                ),
                new Coordinate(
                    21.210699647623727,
                    45.76293956526882
                ),
                new Coordinate(
                    21.21830178599754,
                    45.76007771860992
                ),
                new Coordinate(
                    21.224323281739004,
                    45.764199781461514
                ),
                new Coordinate(
                    21.225678118281053,
                    45.76351716789085
                ),
                new Coordinate(
                    21.227785641790433,
                    45.75981515651918
                ),
                new Coordinate(
                    21.23245230098979,
                    45.7584498137293
                ),
                new Coordinate(
                    21.233581331441513,
                    45.758029701534184
                ),
                new Coordinate(
                    21.233920040577004,
                    45.757005664807224
                ),
                new Coordinate(
                    21.23373186883407,
                    45.75566651149978
                ),
                new Coordinate(
                    21.232527569686113,
                    45.75301436795132
                ),
                new Coordinate(
                    21.235952295390092,
                    45.74923287602783
                ),
                new Coordinate(
                    21.235274877119053,
                    45.748471294576376
                ),
                new Coordinate(
                    21.236333461675457,
                    45.74443022599078
                ),
                new Coordinate(
                    21.23844425707992,
                    45.74536055920839
                )
            }
        };
        
        Render(feature, 300, 300);
        Console.WriteLine("Done!");
    }
}
