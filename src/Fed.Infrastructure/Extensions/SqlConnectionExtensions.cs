using Dapper;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Extensions
{
    public static class SqlConnectionExtensions
    {
        public static async Task<T> ReadJsonAsync<T>(this SqlConnection connection, string sql, object obj = null)
        {
            await connection.OpenAsync();

            var result = await connection.ExecuteScalarAsync<string>(sql, obj);

            if (result == null)
                return default(T);

            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
