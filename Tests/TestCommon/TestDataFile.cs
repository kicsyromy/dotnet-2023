using Mapster.Common.MemoryMappedTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCommon;

[TestClass]
public class TestClass
{
    [TestMethod]
    public void TestDataFile()
    {
        var dataFile = new DataFile("MapData/andorra-10032022.bin");
        dataFile.ForeachFeature(
            new BoundingBox(
                new Coordinate(42.39202286040115, 1.3300323486328125),
                new Coordinate(42.70968691975666, 1.8560028076171875)
            ),
            featureData => { return true; }
        );
    }
}
