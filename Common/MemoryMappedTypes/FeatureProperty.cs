namespace Mapster.Common.MemoryMappedTypes;

[Flags]
public enum FeatureProperty
{
    None = 0,
    Highway = 1 << 0,
    Water = 1 << 1,
    Border = 1 << 2,
    PopulatedPlaceTrue = 1 << 3,
    Railway = 1 << 4,
    PlainNatural = 1 << 5,
    ForestBoundary = 1 << 6,
    ForestLanduse = 1 << 7,
    ResidentialLanduse = 1 << 8,
    PlainLanduse = 1 << 9,
    ReservoirLanduse = 1 << 10,
    Building = 1 << 11,
    Leisure = 1 << 12,
    Amenity = 1 << 13,
    ForestNatural = 1 << 14,
    DesertNatural = 1 << 15,
    MountainNatural = 1 << 16,
    WaterNatural = 1 << 17,
    PopulatedPlaceFalse = 1 << 18


}