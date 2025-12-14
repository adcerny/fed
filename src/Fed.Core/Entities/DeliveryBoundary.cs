using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Fed.Tests.Core")]

namespace Fed.Core.Entities
{
    public class DeliveryBoundary
    {
        public DeliveryBoundary()
        {
            MapCoordinates = new List<MapCoordinate>();
        }

        public DeliveryBoundary(List<MapCoordinate> mapCoordinates)
        {
            MapCoordinates = mapCoordinates;
        }

        public List<MapCoordinate> MapCoordinates { get; set; }

        public Guid HubId { get; set; }

        public string Name { get; set; }

        public bool IsInside(MapCoordinate coordinate)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = MapCoordinates.Count - 1; i < MapCoordinates.Count; j = i++)
            {
                if ((((MapCoordinates[i].Latitude <= coordinate.Latitude) && (coordinate.Latitude < MapCoordinates[j].Latitude))
                        || ((MapCoordinates[j].Latitude <= coordinate.Latitude) && (coordinate.Latitude < MapCoordinates[i].Latitude)))
                        && (coordinate.Longitude < (MapCoordinates[j].Longitude - MapCoordinates[i].Longitude) * (coordinate.Latitude - MapCoordinates[i].Latitude)
                            / (MapCoordinates[j].Latitude - MapCoordinates[i].Latitude) + MapCoordinates[i].Longitude))

                    c = !c;
            }
            return c;
        }
    }
}