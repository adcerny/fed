namespace Fed.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string NormaliseSpace(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return System.Text.RegularExpressions.Regex.Replace(value, @"\s+", " ").Trim();
        }
    }
}
