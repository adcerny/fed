using Fed.Core.Exceptions;
using System;

namespace Fed.Core.ValueTypes
{
    public struct DateRange
    {
        public readonly Date From;
        public readonly Date To;

        private DateRange(Date? from, Date? to)
        {
            var fromDate = from ?? Date.MinDate;
            var toDate = to ?? Date.MaxDate;

            if (to < from)
                throw new IvalidDateRangeException(fromDate, toDate);

            From = fromDate;
            To = toDate;
        }

        public static DateRange TodayUntilEnd() => new DateRange(Date.Today, Date.MaxDate);

        public static DateRange SingleDay(Date day) => new DateRange(day, day);
        public static DateRange Create(Date from, Date to) => new DateRange(from, to);
        public static DateRange Create(string fromDate, string toDate) => Create(Date.Parse(fromDate), Date.Parse(toDate));

        public static DateRange Create(DateTime? from, DateTime? to) =>
            new DateRange(
                from.HasValue ? Date.Create(from.Value) : null,
                to.HasValue ? Date.Create(to.Value) : null);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (23 * hash) + From.GetHashCode();
                hash = (23 * hash) + To.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj) =>
            (obj is DateRange)
            && From.Equals(((DateRange)obj).From)
            && To.Equals(((DateRange)obj).To);

        public override string ToString() => $"{From} - {To}";

        public static bool operator ==(DateRange dateRange1, DateRange dateRange2) => dateRange1.Equals(dateRange2);
        public static bool operator !=(DateRange dateRange1, DateRange dateRange2) => !dateRange1.Equals(dateRange2);

        public bool Overlap(DateRange other) => From <= other.To && To >= other.From;
    }
}