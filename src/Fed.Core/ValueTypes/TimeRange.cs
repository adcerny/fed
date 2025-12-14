using Fed.Core.Exceptions;

namespace Fed.Core.ValueTypes
{
    public struct TimeRange
    {
        public readonly int EarliestHour;
        public readonly int LatestHour;

        private TimeRange(int earliestHour, int latestHour)
        {
            ValidateHour(earliestHour);
            ValidateHour(latestHour);

            if (latestHour < earliestHour)
                throw new InvalidTimeRangeException(earliestHour, latestHour);

            EarliestHour = earliestHour;
            LatestHour = latestHour;
        }

        private static bool IsValidHour(int hour) => hour >= 0 && hour <= 23;

        private static void ValidateHour(int hour)
        {
            if (!IsValidHour(hour))
                throw new InvalidHourException(hour);
        }

        public static TimeRange Create(int earliestHour, int latestHour) =>
            new TimeRange(earliestHour, latestHour);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (23 * hash) + EarliestHour.GetHashCode();
                hash = (23 * hash) + LatestHour.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj) =>
            (obj is TimeRange)
            && EarliestHour.Equals(((TimeRange)obj).EarliestHour)
            && LatestHour.Equals(((TimeRange)obj).LatestHour);

        public override string ToString() => $"{EarliestHour.ToString("00")}:00 - {LatestHour.ToString("00")}:00";

        public static bool operator ==(TimeRange timeRange1, TimeRange timeRange2) => timeRange1.Equals(timeRange2);
        public static bool operator !=(TimeRange timeRange1, TimeRange timeRange2) => !timeRange1.Equals(timeRange2);
    }
}
