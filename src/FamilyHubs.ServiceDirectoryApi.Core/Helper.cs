using GeoCoordinatePortable;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace fh_service_directory_api.core;

public class Helper
{
    public static Point CreatePoint(double latitude, double longitude)
    {
        // 4326 is most common coordinate system used by GPS/Maps
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        // see https://docs.microsoft.com/en-us/ef/core/modeling/spatial
        // Longitude and Latitude
        var newLocation = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

        return newLocation;
    }

    //return distance in meters https://stackoverflow.com/questions/6366408/calculating-distance-between-two-latitude-and-longitude-geocoordinates
    public static double GetDistance(double? latitude1, double? longitude1, double? latitude2, double? longitude2, string? name = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            System.Diagnostics.Debug.WriteLine(name);
        }
        latitude1 ??= 0.0;
        longitude1 ??= 0.0;
        latitude2 ??= 0.0;
        longitude2 ??= 0.0;

        GeoCoordinate pin1 = new(latitude1.Value, longitude1.Value);
        GeoCoordinate pin2 = new(latitude2.Value, longitude2.Value);

        return pin1.GetDistanceTo(pin2);
    }
}
