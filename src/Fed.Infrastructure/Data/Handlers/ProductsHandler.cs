using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class ProductsHandler : IProductsHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public ProductsHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<IList<Product>> ExecuteAsync(GetProductsQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var additionalCondition =
                    query.IncludeDeleted
                    ? ""
                    : " AND [p].[IsDeleted] = 0 AND ([c].[Id] IS NULL OR [c].[IsDeleted] = 0)";

                var lookup = new Dictionary<string, Product>();

                var sql = await SqlQueryReader.GetSqlQueryAsync("GetProducts") + additionalCondition;

                var result = await connection.QueryAsync<Product, Product, Product>(
                   sql, (p, c) =>
                   {
                       Product product;
                       if (!lookup.TryGetValue(p.Id, out product))
                       {
                           lookup.Add(p.Id, product = p);
                       }
                       if (product.ChildProducts == null)
                           product.ChildProducts = new List<Product>();
                       product.ChildProducts.Add(c);
                       return product;
                   }, query
                );

                return lookup.Values.ToList();
            }
        }

        public async Task<bool> ExecuteAsync(UpdateCommand<Product> updateCommand)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var updateProductSql = await SqlQueryReader.GetSqlQueryAsync("UpdateProduct");
                        var deleteProductChildrenSql = await SqlQueryReader.GetSqlQueryAsync("DeleteProductChildren");


                        await connection.ExecuteAsync(
                            updateProductSql,
                            updateCommand.Object,
                            transaction);

                        await connection.ExecuteAsync(
                            deleteProductChildrenSql,
                            updateCommand.Object,
                            transaction);

                        if (updateCommand.Object.ChildProducts != null)
                        {
                            var insertProductChildrenSql = await SqlQueryReader.GetSqlQueryAsync("InsertProductChildren");
                            await connection.ExecuteAsync(
                                insertProductChildrenSql,
                                updateCommand.Object.ChildProducts.Select(c => new
                                {
                                    ProductId = updateCommand.Object.Id,
                                    ChildProductId = c.Id
                                }),
                                transaction);
                        }



                        transaction.Commit();

                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> ExecuteAsync(CreateCommand<Product> createCommand)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {

                    try
                    {
                        var insertSql = await SqlQueryReader.GetSqlQueryAsync("InsertProduct");

                        await connection.ExecuteAsync(
                            insertSql,
                            createCommand.Object,
                            transaction);

                        if (createCommand.Object.ChildProducts != null)
                        {
                            string query = await SqlQueryReader.GetSqlQueryAsync("InsertProductChildren");
                            connection.Execute(query,
                                createCommand.Object.ChildProducts.Select(
                                    c => new { ProductId = createCommand.Object.Id, ChildProductId = c.Id }
                                    ),
                                transaction
                                );
                        }

                        transaction.Commit();

                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> ExecuteAsync(DeleteCommand<string> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var updateSql = await SqlQueryReader.GetSqlQueryAsync("DeleteProduct");

                        await connection.ExecuteAsync(
                            updateSql,
                            new { ProductId = cmd.Id },
                            transaction);

                        transaction.Commit();

                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<Product> ExecuteAsync(GetByIdQuery<Product> obj)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                string query = await SqlQueryReader.GetSqlQueryAsync("GetProductById");

                var lookup = new Dictionary<string, Product>();

                var result = await connection.QueryAsync<Product, Product, Product>(
                    query,
                    (p, c) =>
                    {
                        Product product;
                        if (!lookup.TryGetValue(p.Id, out product))
                        {
                            lookup.Add(p.Id, product = p);
                        }
                        if (product.ChildProducts == null)
                            product.ChildProducts = new List<Product>();
                        if(c != null)
                            product.ChildProducts.Add(c);
                        return product;
                    }, obj
                );

                return lookup.Values.Single();
            }
        }
    }
}