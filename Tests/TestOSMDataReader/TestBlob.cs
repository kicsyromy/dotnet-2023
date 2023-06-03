using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using OSMDataParser;
using OSMDataParser.Elements;

namespace TestOSMDataReader;

[TestClass]
public class TestBlob
{
    [TestMethod]
    public void TestLoad()
    {
        var nodes = new ConcurrentDictionary<long, AbstractNode>();
        var ways = new ConcurrentBag<Way>();

        foreach (var blob in new PBFFile("MapData/andorra-10032022.osm.pbf"))
        {
            switch (blob.Type)
            {
                case BlobType.Primitive:
                    {
                        var primitiveBlock = blob.ToPrimitiveBlock();
                        foreach (var primitiveGroup in primitiveBlock)
                        {
                            switch (primitiveGroup.ContainedType)
                            {
                                case PrimitiveGroup.ElementType.Node:
                                    foreach (var node in primitiveGroup)
                                    {
                                        nodes[node.Id] = (AbstractNode)node;
                                    }
                                    break;

                                case PrimitiveGroup.ElementType.Way:
                                    foreach (var way in primitiveGroup)
                                    {
                                        ways.Add((Way)way);
                                    }
                                    break;
                            }
                        }
                        break;
                    }
            }
        }

        foreach (var way in ways)
        {
            foreach (var nodeId in way.NodeIds)
            {
                if (!nodes.ContainsKey(nodeId))
                {
                    Console.Error.WriteLine($"Missing node: {nodeId}");
                }
            }
        }
    }
}