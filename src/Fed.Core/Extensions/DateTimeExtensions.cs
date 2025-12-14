using Fed.Core.Enums;
using Fed.Core.ValueTypes;
using System;
using System.Globalization;

namespace Fed.Core.Extensions
{
    public static class DateTimeExtensions
    {
        
        public readonly static TimeSpan DailyCutOffTime = new TimeSpan(16, 0, 0);

        const int daysInWeek = 7;

        public static DateTime AddWeeks(this DateTime dateTime, WeeklyRecurrence weeklyRecurrence) => dateTime.AddWeeks((int)weeklyRecurrence);
        public static DateTime AddWeeks(this DateTime dateTime, int numberOfWeeks) => dateTime.AddDays(daysInWeek * numberOfWeeks);
        public static Date ToDate(this DateTime dateTime) => Date.Create(dateTime);


        /// <summary>
        /// Returns the next working day after a given date.
        /// </summary>
        public static DateTime GetNextWorkingDay(this DateTime dateTime)
        {
            if (dateTime.DayOfWeek == DayOfWeek.Friday)
                return dateTime.AddDays(3);
            else if (dateTime.DayOfWeek == DayOfWeek.Saturday)
                return dateTime.AddDays(2);
            else
                return dateTime.AddDays(1);
        }

        /// <summary>
        /// Returns the next working day after a given date.
        /// </summary>
        public static DateTime GetNextAvailableWorkingDay()
        {
            var now = Now().ToBritishTime();
            var nextDay = now.GetNextWorkingDay();
            if(now.IsAfterCutOffTime())
                nextDay = nextDay.GetNextWorkingDay();
            return nextDay.Date;
        }

        /// <summary>
        /// Returns the first available delivery date taking into account the daily cut off time.
        /// </summary>
        public static DateTime GetNextAvailableDeliveryDate(this DateTime dateTime)
        {
            var now = Now().ToBritishTime();
            var earliestDate = now.GetNextWorkingDay().Date;

            if (dateTime.DayOfWeek == DayOfWeek.Saturday || 
                dateTime.DayOfWeek == DayOfWeek.Sunday)
                dateTime = dateTime.GetNextWorkingDay();

            var nextDate = dateTime < earliestDate ? earliestDate.NextWeekday(dateTime.DayOfWeek) : dateTime;

            if (now.IsAfterCutOffTime() && nextDate.Date == now.GetNextWorkingDay().Date)
                nextDate = nextDate.AddWeeks(1);

            return nextDate;
        }

        /// <summary>
        /// Returns the next future weekday available delivery date
        /// </summary>
        public static DateTime NextWeekday(this DateTime dateTime, DayOfWeek day, bool includeToday = true)
        {
            int i = includeToday ? 0 : 1;

            var date = (dateTime < Today().AddDays(i) ? Today().AddDays(i) : dateTime);
            
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)date.DayOfWeek + 7) % 7;
            return date.AddDays(daysToAdd);
        }

        public static DateTime AddWorkingDays(this DateTime dateTime, int count)
        {
            while (count-- > 0)
                dateTime = dateTime.GetNextWorkingDay();

            return dateTime;
        }

        public static DateTime GetPreviousWorkingDay(this DateTime dateTime)
        {
            switch(dateTime.Date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return dateTime.AddDays(-3);
                case DayOfWeek.Sunday:
                    return dateTime.AddDays(-2);
                default:
                    return dateTime.AddDays(-1);
            }
        }

        public static DateTime ToBritishTime(this DateTime dateTime)
        {
            var britishZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Local, britishZone);
        }

        public static bool IsAfterCutOffTime(this DateTime dateTime)
        {
            //if it's the weekend, we are past cutoff
            if (dateTime.DayOfWeek == DayOfWeek.Saturday ||
                dateTime.DayOfWeek == DayOfWeek.Sunday)
                return true;

            var cutOff = Today() + DailyCutOffTime;

            return dateTime.ToBritishTime() >= cutOff;
        }

        public static DateTime EquivalentWeekDay(this DateTime date, DayOfWeek day) =>
            date.Equals(DateTime.MaxValue.Date) ?
                DateTime.MaxValue.Date :
                date.AddDays((int)day - (int)date.DayOfWeek);

        public static DateTime MondayOfPreviousWeek(this DateTime date) =>
            date.AddDays(-(int)date.DayOfWeek - 6);

        public static int WeekNumber(this DateTime date) =>
            CultureInfo.GetCultureInfo("en-GB").Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        public static int ToIsoWeekOfYear(this DateTime dateTime)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dateTime);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                dateTime = dateTime.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static Func<DateTime> Now = () => DateTime.Now;
        public static Func<DateTime> Today = () => Now().Date;

    }

}

