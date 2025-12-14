using Fed.Core.Enums;
using Fed.Core.Exceptions;
using Fed.Core.Extensions;
using System;

namespace Fed.Core.ValueTypes
{
    public struct Date : IConvertible, IComparable
    {
        const int daysInWeek = 7;
        public readonly DateTime Value;

        private enum MinOrMaxDate
        {
            MinDate,
            MaxDate
        }

        private Date(MinOrMaxDate minOrMaxDate)
        {
            Value =
                minOrMaxDate == MinOrMaxDate.MinDate
                    ? new DateTime(1900, 1, 1).Date
                    : DateTime.MaxValue.Date;
        }

        private Date(DateTime dateTime)
        {
            if (dateTime < MinDate.Value || dateTime > MaxDate.Value)
                throw new DateOutOfRangeException(dateTime);

            Value = dateTime.Date;
        }

        public static Date Create(DateTime date) => new Date(date);
        public static Date Create(int year, int month, int day) => new DateTime(year, month, day).ToDate();
        public static Date Parse(string date) => Create(DateTime.Parse(date));

        public static Date Today => DateTime.Today.ToDate();
        public static Date MinDate => new Date(MinOrMaxDate.MinDate);
        public static Date MaxDate => new Date(MinOrMaxDate.MaxDate);

        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString("yyyy-MM-dd");

        public override bool Equals(object obj) => (obj is Date) && Value.Equals(((Date)obj).Value);

        public static bool operator ==(Date date1, Date date2) => date1.Equals(date2);
        public static bool operator !=(Date date1, Date date2) => !date1.Equals(date2);
        public static bool operator >(Date date1, Date date2) => date1.Value > date2.Value;
        public static bool operator <(Date date1, Date date2) => date1.Value < date2.Value;
        public static bool operator >=(Date date1, Date date2) => date1.Value >= date2.Value;
        public static bool operator <=(Date date1, Date date2) => date1.Value <= date2.Value;

        public Date AddWeeks(WeeklyRecurrence weeklyRecurrence) => AddWeeks((int)weeklyRecurrence);
        public Date AddWeeks(int weeks) => Create(Value.AddDays(daysInWeek * weeks));
        public Date AddMonths(int months) => Create(Value.AddMonths(months));
        public Date AddDays(int days) => Create(Value.AddDays(days));

        public static implicit operator Date(DateTime d) => Create(d);
        public static implicit operator DateTime(Date d) => d.Value;

        public static implicit operator Date(string d) => Parse(d);
        public static implicit operator string(Date d) => d.ToString();

        public TypeCode GetTypeCode() => TypeCode.Object;
        public object ToType(Type conversionType, IFormatProvider provider) => Convert.ChangeType(Value, conversionType);
        public DateTime ToDateTime(IFormatProvider provider) => Value;
        public string ToString(IFormatProvider provider) => ToString();

        public bool ToBoolean(IFormatProvider provider) => throw new InvalidCastException();
        public byte ToByte(IFormatProvider provider) => throw new InvalidCastException();
        public char ToChar(IFormatProvider provider) => throw new InvalidCastException();
        public decimal ToDecimal(IFormatProvider provider) => throw new InvalidCastException();
        public double ToDouble(IFormatProvider provider) => throw new InvalidCastException();
        public short ToInt16(IFormatProvider provider) => throw new InvalidCastException();
        public int ToInt32(IFormatProvider provider) => throw new InvalidCastException();
        public long ToInt64(IFormatProvider provider) => throw new InvalidCastException();
        public sbyte ToSByte(IFormatProvider provider) => throw new InvalidCastException();
        public float ToSingle(IFormatProvider provider) => throw new InvalidCastException();
        public ushort ToUInt16(IFormatProvider provider) => throw new InvalidCastException();
        public uint ToUInt32(IFormatProvider provider) => throw new InvalidCastException();
        public ulong ToUInt64(IFormatProvider provider) => throw new InvalidCastException();

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var date = (Date)obj;

            return Value.CompareTo(date.Value);
        }
    }
}