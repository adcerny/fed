using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data
{
    public static class SqlQueryReader
    {
        private static string GetResourceName(string sqlFileName) =>
            sqlFileName.EndsWith(".sql")
                ? $"Fed.Infrastructure.SQL.{sqlFileName}"
                : $"Fed.Infrastructure.SQL.{sqlFileName}.sql";

        private static Stream GetManifestStream(string sqlFileName) =>
            typeof(SqlQueryReader).GetTypeInfo().Assembly.GetManifestResourceStream(GetResourceName(sqlFileName));

        public static async Task<string> GetSqlQueryAsync(string sqlFileName)
        {
            using (var stream = GetManifestStream(sqlFileName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
