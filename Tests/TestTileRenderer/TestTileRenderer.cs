using System.Collections.Generic;
using Mapster.Common.MemoryMappedTypes;
using Mapster.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;

namespace TestTileRenderer;

[TestClass]
public class TestTileRenderer
{
    [TestMethod]
    public void TestRendering()
    {
        var dataFile = new DataFile("MapData/andorra-10032022.bin");

        var pixelBb = new TileRenderer.BoundingBox
        {
            MinX = float.MaxValue,
            MinY = float.MaxValue,
            MaxX = float.MinValue,
            MaxY = float.MinValue
        };
        var shapes = new PriorityQueue<BaseShape, int>();
        dataFile.ForeachFeature(
            new BoundingBox(
                new Coordinate(42.39202286040115, 1.3300323486328125),
                new Coordinate(42.70968691975666, 1.8560028076171875)
            ),
            featureData =>
            {
                featureData.Tessellate(ref pixelBb, ref shapes);
                return true;
            }
        );

        var image = shapes.Render(pixelBb, 8000, 8000);
        image.SaveAsPng("output.png");
    }
}
