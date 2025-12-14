using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Infrastructure.Data.SqlServer;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.Handlers
{
    public class ProductCategoriesHandler : IProductCategoriesHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public ProductCategoriesHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<ProductCategory> ExecuteAsync(GetByIdQuery<ProductCategory> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                const string sql = "SELECT * FROM [dbo].[ProductCategories] where id=@id";
                var result = await connection.QueryFirstAsync<ProductCategory>(sql, new { query.Id });

                return result;
            }
        }

        public async Task<IList<ProductCategory>> ExecuteAsync(GetAllQuery<ProductCategory> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                const string sql = "SELECT * FROM [dbo].[ProductCategories]";
                var result = await connection.QueryAsync<ProductCategory>(sql, query);

                return result.ToList();
            }
        }        

        public async Task<bool> ExecuteAsync(CreateCommand<ProductCategory> createCommand)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = await SqlQueryReader.GetSqlQueryAsync("InsertProductCategory.sql");

                var result = await connection.ExecuteAsync(sql, createCommand.Object);

                return true;
            }
        }

        public async Task<bool> ExecuteAsync(UpdateCommand<ProductCategory> updateCommand)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("UpdateProductCategory.sql");

                var obj = new
                {
                    updateCommand.Object.Id,
                    updateCommand.Object.Name

                };
                await connection.ExecuteAsync(sql, obj);
                return true;
            }
        }      

        public async Task<bool> ExecuteAsync(CreateCommand<ProductCategoryProducts> command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("InsertProductCategoryProduct.sql");

                var obj = new
                {
                    command.Object.ProductCategoryId,
                    command.Object.ProductId

                };
                await connection.ExecuteAsync(sql, obj);
                return true;
            }
        }
       
        public async Task<IList<ProductCategoryProducts>> ExecuteAsync(GetByIdQuery<ProductCategoryProducts> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                const string sql = "SELECT * FROM [dbo].[ProductCategoryProducts] where productid=@id";
                var result = await connection.QueryAsync<ProductCategoryProducts>(sql, new { query.Id });

                return result.ToList();
            }
        }
    }
}
