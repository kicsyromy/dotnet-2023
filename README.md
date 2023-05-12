Mapster
-----------------------
The solution in this repository contains several projects that aim to read and extract information from the OpenStreeMap binary [`osm.pbf`](https://wiki.openstreetmap.org/wiki/PBF_Format) format, as well as interpret and present this data in a user-facing way.

To this end the solution contains several parts that are used at different times in the lifetime of the application.

## General Overview

### I. Common - Library
A common set of data structures used throughout the codebase.

### II. DataPipeline
#### 1. OSMDataParser - Library
Reads, parses and interprets data from an OpenStreetMap binary file in [PBF](https://wiki.openstreetmap.org/wiki/PBF_Format) format. It extensively uses iterators in order to extract data on an as-needed basis and optimizes for memory efficiency rather than CPU usage efficiency. That being said it supports reading per-blob data in parallel, which can speed up parsing significantly.

It is used by the MapFeatureGenerator to extract [Way](https://wiki.openstreetmap.org/wiki/Way) and [Node](https://wiki.openstreetmap.org/wiki/Node) information from a [PBF](https://wiki.openstreetmap.org/wiki/PBF_Format) file.

#### 2. MapFeatureGenerator - Executable
Uses OSMDataParser to read mapping information from a PBF file and outputs it to a format that can be used by the service to serve map data.

The main work is done in `CreateMapDataFile` that generates a binary file that can be mapped into process memory by the service making load times for the services practically instant.

This executable would be run in an automated fashion in order to synchronize with any updates to the OSM data.
In order to run the application one should provide the input `.osm.pbf` file as well as a name for the output file that will be generated.

### III. Rendering
#### 1. TileRenderer - Library
Used to tessellate and render a set of map features.

The two most important methods are the two extension methods:
  - `Tessellate` - That creates a BaseShape with unbound pixel coordinates for a map feature
  - `Render` - That takes a collection of shapes and, based on their Z index, renders them, scaled, to a RGBA image

### IV. Service
#### 1. Service - Executable
A .NET MinimalAPI application with, currently, a single endpoint `/render` that takes a bounding box and an, optional, size and renders a png image with the geographical features contained within the bounding box.

### V. Client
#### 1. ClientApplication
This is the client/user facing part of the application and it is responsible to creating and sending requests to the backend/service.
Since this project is only at POC stage it just renders a window with the tile for Andorra.

### VI. Tests
#### 1. TestCommon
Tests for reading and interpreting a DataFile.
#### 2. TestOSMDataReader
Tests for reading and parsing a `.osm.pbf` file.
#### 3. TestTileRenderer
Tests for rendering a file resulted from MapFeatureGenerator.

## Running End-to-End
### Generating Data
First thing in order to serve up map data, there needs to be data to serve.

As stated in the general overview data is generated from OpenStreetMap [PBF](https://wiki.openstreetmap.org/wiki/PBF_Format) files which are deserialized and transformed into a internal representation conducive to memory mapping.

Assuming the PWD/CWD is the root of the repository and the `dotnet` executable is in PATH:
```shell
cd DataPipeline/MapFeatureGenerator
dotnet run -- -i <path/to/input.osm.pbf> -o <path/to/output.bin>
```

As an example running with the PBF file for Andorra (located in the repository):
```shell
cd DataPipeline/MapFeatureGenerator
dotnet run -- -i "../../Tests/TestOSMDataReader/MapData/andorra-10032022.osm.pbf" -o "../../../andorra.bin"
```

### Running the backend service
Once data is generated the service can be started:

Assuming the PWD/CWD is the root of the repository and the `dotnet` executable is in PATH:
```shell
cd Service/Service
dotnet run -- -i <path/to/input.osm.pbf>
```

Building on the previous example, and using the output generated there:
```shell
cd Service/Service
dotnet run -- -i "../../../andorra.bin"
```

The preprocessed data used in the tests can also be used:
```shell
cd Service/Service
dotnet run -- -i "../../Tests/TestTileRenderer/MapData/andorra-10032022.bin"
```

Once the service is up and running it will serve the `/render` endpoint on port `8080` and will have Andorra available as a data set.

The service can be tested using any API testing tools like [Postman](https://www.postman.com/), [Insomnia](https://insomnia.rest/) or even a simple web browser. To get the entirety of Andorra to render, at a resolution of 2000x2000, issue the following `GET` request:
```http request
http://localhost:8080/render?minLat=42.39202286040&minLon=1.33003234863281&maxLat=42.709686919756&maxLon=1.85600280761718&size=2000
```
Alternatively run this from your favourite shell (CMD, bash, zsh, etc) to download the rendered bounding box:
```shell
curl -L "http://localhost:8080/render?minLat=42.39202286040&minLon=1.33003234863281&maxLat=42.709686919756&maxLon=1.85600280761718&size=2000" -o rendered.png
```

### Running the client application
Finally if working on the client application this can, at this point, be started.

Assuming the PWD/CWD is the root of the repository and the `dotnet` executable is in PATH:
```shell
cd Client/ClientApplication
dotnet run
```

Clicking the *Render* button will make a request to the backend and will render the response in the window frame.

## Running Tests
Tests can be run from the root of the repository or from each individual test project folder by issuing:
```shell
dotnet test
```