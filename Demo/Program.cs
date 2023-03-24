using System.Buffers.Binary;
using System.IO.Compression;
using OSMPBF;

FileStream fs = new FileStream("/var/home/kicsyromy/Downloads/romania-latest.osm.pbf", FileMode.Open);

// Allocate enough bytes to hold a 32bit int
byte[] buffer = new byte[4];
// Read from the stream into the buffer
fs.Read(buffer, 0, buffer.Length);
// Interpret the bytes as a big-endian 32bit int (https://docs.microsoft.com/en-us/dotnet/api/system.buffers.binary.binaryprimitives?view=net-6.0)
int headerSize = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan());

// Allocate enough bytes to hold the serialized header (headerSize)
byte[] headerBuffer = new byte[headerSize];
// Read the serialized header into the buffer
fs.Read(headerBuffer, 0, headerBuffer.Length);
// Deserialize the header
BlobHeader blobHeader = BlobHeader.Parser.ParseFrom(headerBuffer);

// Allocate enough bytes to hold the serialized blob (blobHeader.Datasize)
byte[] blobBuffer = new byte[blobHeader.Datasize];
// Read the serialized blob into the buffer
fs.Read(blobBuffer, 0, blobBuffer.Length);
// Deserialize the blob
Blob blob = Blob.Parser.ParseFrom(blobBuffer);

// The blob contains ZLib compressed data, hold reference to those bytes
var compressedData = blob.ZlibData.Span;
// Create a MemoryStream that uses the compressed bytes as a source
MemoryStream ms = new MemoryStream(compressedData.ToArray());
// Overlay a ZLib decompression stream on top of the memory stream do decompress the
// data on the fly
ZLibStream zlibStream = new ZLibStream(ms, CompressionMode.Decompress);
// Deserialze a HeaderBlock from the compressed stream
HeaderBlock headerBlock = HeaderBlock.Parser.ParseFrom(zlibStream);
Console.WriteLine(headerBlock);