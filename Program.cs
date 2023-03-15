using Google.Protobuf;

{
    Node n1 = new Node();
    n1.Id = 1;
    n1.Lat = 2;
    n1.Lon = 3;

    using var stream = new FileStream("node.bin", FileMode.Create);
    n1.WriteTo(stream);
    stream.Flush();
}

var node2 = Node.Parser.ParseFrom(File.ReadAllBytes("node.bin"));
Console.WriteLine(node2);