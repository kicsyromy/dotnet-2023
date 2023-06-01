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
        var properties = feature.Properties;

        if ((properties & FeatureProperty.Highway) == FeatureProperty.Highway)
        {
            var road = new Road(feature.Coordinates);
            baseShape = road;
            shapes.Enqueue(road, road.ZIndex);
        }
        else if ((properties & FeatureProperty.Water) == FeatureProperty.Water && featureType != GeometryType.Point)
        {
            var waterway = new Waterway(feature.Coordinates, featureType == GeometryType.Polygon);
            baseShape = waterway;
            shapes.Enqueue(waterway, waterway.ZIndex);
        }
        else if ((properties & FeatureProperty.Border) == FeatureProperty.Border)
        {
            var border = new Border(feature.Coordinates);
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
        else if ((properties & FeatureProperty.Railway) == FeatureProperty.Railway)
        {
        var railway = new Railway(feature.Coordinates);
        baseShape = railway;
        shapes.Enqueue(railway, railway.ZIndex);
        }
     else if ((properties & FeatureProperty.ForestNatural ) == FeatureProperty.ForestNatural || 
     (properties & FeatureProperty.DesertNatural) == FeatureProperty.DesertNatural || 
     (properties & FeatureProperty.MountainNatural) == FeatureProperty.MountainNatural || 
     (properties & FeatureProperty.WaterNatural) == FeatureProperty.WaterNatural)
    {
        var geoFeature = new GeoFeature(feature.Coordinates, feature);
        baseShape = geoFeature;
        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
    }
    else if ((properties & FeatureProperty.ForestBoundary) == FeatureProperty.ForestBoundary)
    {
        var geoFeature = new GeoFeature(feature.Coordinates, GeoFeature.GeoFeatureType.Forest);
        baseShape = geoFeature;
        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
    }
    else if ((properties & FeatureProperty.ForestLanduse) == FeatureProperty.ForestLanduse)
    {
        var geoFeature = new GeoFeature(feature.Coordinates, GeoFeature.GeoFeatureType.Forest);
        baseShape = geoFeature;
        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
    }
    else if ((properties & FeatureProperty.ResidentialLanduse) == FeatureProperty.ResidentialLanduse)
    {
        var geoFeature = new GeoFeature(feature.Coordinates, GeoFeature.GeoFeatureType.Residential);
        baseShape = geoFeature;
        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
    }
    else if ((properties & FeatureProperty.PlainLanduse) == FeatureProperty.PlainLanduse)
    {
        var geoFeature = new GeoFeature(feature.Coordinates, GeoFeature.GeoFeatureType.Plain);
        baseShape = geoFeature;
        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
    }
    else if ((properties & FeatureProperty.ReservoirLanduse) == FeatureProperty.ReservoirLanduse)
    {
        var geoFeature = new GeoFeature(feature.Coordinates, GeoFeature.GeoFeatureType.Water);
        baseShape = geoFeature;
        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
    }
    else if ((properties & FeatureProperty.Building) == FeatureProperty.Building)
    {
        var geoFeature = new GeoFeature(feature.Coordinates, GeoFeature.GeoFeatureType.Residential);
        baseShape = geoFeature;
        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
    }
    else if ((properties & FeatureProperty.Leisure) == FeatureProperty.Leisure)
    {
        var geoFeature = new GeoFeature(feature.Coordinates, GeoFeature.GeoFeatureType.Residential);
        baseShape = geoFeature;
        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
    }
     else if ((properties & FeatureProperty.Amenity) == FeatureProperty.Amenity)
    {
        var geoFeature = new GeoFeature(feature.Coordinates, GeoFeature.GeoFeatureType.Residential);
        baseShape = geoFeature;
        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
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
            // FIXME: Hack
            if (entry.ScreenCoordinates.Length < 2)
            {
                continue;
            }
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
