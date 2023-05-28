using Mapster.Common.MemoryMappedTypes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Mapster.Rendering;

public static class TileRenderer
{
    public static BaseShape Tessellate(this MapFeatureData feature, ref BoundingBox boundingBox, ref PriorityQueue<BaseShape, int> shapes)
    {
        BaseShape? baseShape = null;

        var featureType = feature.Type;
        if (Border.ShouldBeBorder(feature))
        {
            var coordinates = feature.Coordinates;
            var border = new Border(coordinates);
            baseShape = border;
            shapes.Enqueue(border, border.ZIndex);
        }
        else if (PopulatedPlace.ShouldBePopulatedPlace(feature))
        {
            var coordinates = feature.Coordinates;
            var popPlace = new PopulatedPlace(coordinates, feature);
            baseShape = popPlace;
            shapes.Enqueue(popPlace, popPlace.ZIndex);
        }
        else
        {
            bool varSemafor = false;
            foreach (var el in feature.Properties)
            {
                if (varSemafor)
                    break;
                ReadOnlySpan<Coordinate> coordinates;
                switch (el.Key)
                {
                    case MapFeatureData.SomeRandomName.Amenity:
                        if (feature.Type == GeometryType.Polygon)
                        {
                            varSemafor = true;
                            coordinates = feature.Coordinates;
                            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                            baseShape = geoFeature;
                            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        }

                        break;

                    case MapFeatureData.SomeRandomName.Boundary:
                        if (el.Value.StartsWith("forest"))
                        {
                            varSemafor = true;
                            coordinates = feature.Coordinates;
                            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Forest);
                            baseShape = geoFeature;
                            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        }

                        break;
                    
                    case MapFeatureData.SomeRandomName.Building:
                        if (feature.Type == GeometryType.Polygon)
                        {
                            varSemafor = true;
                            coordinates = feature.Coordinates;
                            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                            baseShape = geoFeature;
                            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        }
                        break;

                    case MapFeatureData.SomeRandomName.Highway:
                        if (MapFeature.HighwayTypes.Any(v => el.Value.StartsWith(v)))
                        {
                            varSemafor = true;
                            coordinates = feature.Coordinates;
                            var road = new Road(coordinates);
                            baseShape = road;
                            shapes.Enqueue(road, road.ZIndex);
                        }

                        break;
                    
                    case MapFeatureData.SomeRandomName.Landuse:
                        if (el.Value.StartsWith("forest") || el.Value.StartsWith("orchard"))
                        {
                            varSemafor = true;
                            coordinates = feature.Coordinates;
                            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Forest);
                            baseShape = geoFeature;
                            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        }
                        else if (feature.Type == GeometryType.Polygon)
                        {
                            if (el.Value.StartsWith("residential") || el.Value.StartsWith("cemetery") ||
                                el.Value.StartsWith("industrial") || el.Value.StartsWith("commercial") ||
                                el.Value.StartsWith("square") || el.Value.StartsWith("construction") ||
                                el.Value.StartsWith("military") || el.Value.StartsWith("quarry") ||
                                el.Value.StartsWith("brownfield"))
                            {
                                varSemafor = true;
                                coordinates = feature.Coordinates;
                                var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                                baseShape = geoFeature;
                                shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                            }
                            else if (el.Value.StartsWith("farm") || el.Value.StartsWith("meadow") ||
                                     el.Value.StartsWith("grass") || el.Value.StartsWith("greenfield") ||
                                     el.Value.StartsWith("recreation_ground") || el.Value.StartsWith("winter_sports")
                                     || el.Value.StartsWith("allotments"))
                            {
                                varSemafor = true;
                                coordinates = feature.Coordinates;
                                var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Plain);
                                baseShape = geoFeature;
                                shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                            }
                            else if (el.Value.StartsWith("reservoir") || el.Value.StartsWith("basin"))
                            {
                                varSemafor = true;
                                coordinates = feature.Coordinates;
                                var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Water);
                                baseShape = geoFeature;
                                shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                            }
                        }

                        break;
                    
                    case MapFeatureData.SomeRandomName.Leisure:
                        if (feature.Type == GeometryType.Polygon)
                        {
                            varSemafor = true;
                            coordinates = feature.Coordinates;
                            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                            baseShape = geoFeature;
                            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        }

                        break;
                    
                    case MapFeatureData.SomeRandomName.Natural:
                        if (featureType == GeometryType.Polygon)
                        {
                            varSemafor = true;
                            coordinates = feature.Coordinates;
                            var geoFeature = new GeoFeature(coordinates, feature);
                            baseShape = geoFeature;
                            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        }

                        break;

                     case MapFeatureData.SomeRandomName.Railway:
                        varSemafor = true;
                        coordinates = feature.Coordinates;
                        var railway = new Railway(coordinates);
                        baseShape = railway;
                        shapes.Enqueue(railway, railway.ZIndex);

                        break;

                    case MapFeatureData.SomeRandomName.Water:
                        if (feature.Type != GeometryType.Point)
                        {
                            varSemafor = true;
                            coordinates = feature.Coordinates;

                            var waterway = new Waterway(coordinates, feature.Type == GeometryType.Polygon);
                            baseShape = waterway;
                            shapes.Enqueue(waterway, waterway.ZIndex);
                        }

                        break;

                    default:
                        break;
                }
            }
        }

        if (baseShape != null)
        {
            for (var j = 0; j < baseShape.ScreenCoordinates.Length; ++j)
            {
                boundingBox.MinX = Math.Min(boundingBox.MinX, baseShape.ScreenCoordinates[j].X);
                boundingBox.MaxX = Math.Max(boundingBox.MaxX, baseShape.ScreenCoordinates[j].X);
                boundingBox.MinY = Math.Min(boundingBox.MinY, baseShape.ScreenCoordinates[j].Y);
                boundingBox.MaxY = Math.Max(boundingBox.MaxY, baseShape.ScreenCoordinates[j].Y);
            }
        }

        return baseShape;
    }

    public static Image<Rgba32> Render(this PriorityQueue<BaseShape, int> shapes, BoundingBox boundingBox, int width, int height)
    {
        var canvas = new Image<Rgba32>(width, height);

        // Calculate the scale for each pixel, essentially applying a normalization
        var scaleX = canvas.Width / (boundingBox.MaxX - boundingBox.MinX);
        var scaleY = canvas.Height / (boundingBox.MaxY - boundingBox.MinY);
        var scale = Math.Min(scaleX, scaleY);

        // Background Fill
        canvas.Mutate(x => x.Fill(Color.White));
        while (shapes.Count > 0)
        {
            var entry = shapes.Dequeue();
            entry.TranslateAndScale(boundingBox.MinX, boundingBox.MinY, scale, canvas.Height);
            canvas.Mutate(x => entry.Render(x));
        }

        return canvas;
    }

    public struct BoundingBox
    {
        public float MinX;
        public float MaxX;
        public float MinY;
        public float MaxY;
    }
}
