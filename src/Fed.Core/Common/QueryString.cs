using System.Collections.Generic;
using System.Linq;

namespace Fed.Core.Common
{
    public static class QueryString
    {
        public static string Create(IDictionary<string, string> keyValuePairs)
        {
            var queryStr = string.Empty;

            if (keyValuePairs == null || keyValuePairs.Count == 0)
                return queryStr;

            var keys = keyValuePairs.Keys.ToList();

            for (var i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                var val = keyValuePairs[key];

                queryStr = i == 0 ? $"?{key}={val}" : $"{queryStr}&{key}={val}";
            }

            return queryStr;
        }
    }
}