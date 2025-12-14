using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Common
{
    public class Base36
    {
        public const int NumberBase = 36;

        private static int RoundToInt(double x) => Convert.ToInt32(Math.Floor(x));
        private static string ToBase36(int x) => x >= 10 ? ((char)(x + 55)).ToString() : x.ToString();

        private static List<int> FromDec(double dec, List<int> nums)
        {
            var q = RoundToInt(dec / NumberBase);
            var r = RoundToInt(dec % NumberBase);

            nums.Add(r);

            return q == 0 ? nums : FromDec(q, nums);
        }

        public static string FromDec(int dec)
        {
            var nums = FromDec((double)dec, new List<int>());
            var sb = new StringBuilder();

            for (var i = nums.Count - 1; i >= 0; i--)
            {
                var num = nums[i];
                sb.Append(ToBase36(num));
            }

            return sb.ToString();
        }
    }
}