using System;

namespace Fed.Core.ValueTypes
{
    public struct MapCoordinate : IEquatable<MapCoordinate>
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public MapCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString() => $"{Latitude},{Longitude}";

        public override bool Equals(object other) => other is MapCoordinate && Equals((MapCoordinate)other);

        public bool Equals(MapCoordinate other) => Latitude == other.Latitude && Longitude == other.Longitude;

        public override int GetHashCode() => Latitude.GetHashCode() ^ Longitude.GetHashCode();
    }
}