using Fed.Core.ValueTypes;
using System;
using System.Text.RegularExpressions;

namespace Fed.Core.Entities
{
    public class PostcodeLocation
    {
        public Guid Id { get; set; }
        public string Postcode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public const string RegEx = @"^(([gG][iI][rR] {0,}0[aA]{2})|((([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y]?[0-9][0-9]?)|(([a-pr-uwyzA-PR-UWYZ][0-9][a-hjkstuwA-HJKSTUW])|([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y][0-9][abehmnprv-yABEHMNPRV-Y]))) {0,1}[0-9][abd-hjlnp-uw-zABD-HJLNP-UW-Z]{2}))$";

        public MapCoordinate Coordinate => new MapCoordinate(Latitude, Longitude);

        public static PostcodeLocation Create(string postcode, double latitude, double longitude) =>
            new PostcodeLocation { Id = Guid.NewGuid(), Postcode = postcode, Latitude = latitude, Longitude = longitude };

        public static bool IsPostcodeValid(string postcode)
        {
            var match = Regex.Match(postcode, RegEx, RegexOptions.IgnoreCase);
            return match.Success;
        }

        public override string ToString() => Address.NormalisePostcode(Postcode);
    }
}