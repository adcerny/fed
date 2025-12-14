using System;

namespace Fed.Infrastructure.Data.SqlServer
{
    public class SqlServerConfig : ISqlServerConfig
    {
        public SqlServerConfig(string connectionString)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public string ConnectionString { get; }
    }
}
