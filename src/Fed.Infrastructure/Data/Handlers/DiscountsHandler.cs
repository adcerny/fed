using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Models;
using Fed.Infrastructure.Data.SqlServer;
using Fed.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class DiscountsHandler : IDiscountsHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public DiscountsHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<Discount> ExecuteAsync(GetByIdQuery<Discount> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var orders = await ExecuteAsync(new GetByIdsQuery<Discount>(new List<string> { query.Id }));
                return orders?.Single();
            }
        }

        public async Task<IList<Discount>> ExecuteAsync(GetByIdsQuery<Discount> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {

                var ut = new DataTable("IdTable");
                ut.Columns.Add("Id");
                foreach (var id in query.Ids)
                    ut.Rows.Add(id);

                var sql = await SqlQueryReader.GetSqlQueryAsync("GetDiscounts");
                var result = await connection.ReadJsonAsync<IList<Discount>>(sql, new { ut = ut.AsTableValuedParameter("IdTable") });
                return result;
            }
        }

        public async Task<IList<Discount>> ExecuteAsync(GetAllQuery<Discount> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetAllDiscounts.sql");

                return await connection.ReadJsonAsync<IList<Discount>>(sql);
            }
        }

        public async Task<IList<CustomerDiscount>> ExecuteAsync(GetDiscountsByCustomerQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetDiscountsByCustomerId.sql");

                return await connection.ReadJsonAsync<IList<CustomerDiscount>>(sql, new { query.CustomerId, query.IncludeUnapplied });
            }
        }

        public async Task<IList<Discount>> ExecuteAsync(GetDiscountsApplicableQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetDiscountsApplicable.sql");

                return await connection.ReadJsonAsync<IList<Discount>>(sql, query);
            }
        }

        public async Task<bool> ExecuteAsync(CreateCommand<CustomerDiscount> createCommand)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("InsertCustomerDiscount.sql");

                var obj =
                new
                {
                    createCommand.Object.CustomerId,
                    createCommand.Object.DiscountId,
                    createCommand.Object.AppliedDate,
                    createCommand.Object.DiscountCode,
                    EndDate = createCommand.Object.EndDate?.Value
                };

                await connection.ExecuteAsync(sql, obj);

                return true;
            }
        }

        public async Task<Discount> ExecuteAsync(UpdateCommand<Discount> updateCommand)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var sql = await SqlQueryReader.GetSqlQueryAsync("UpdateDiscount.sql");

                        var obj =
                        new
                        {
                            updateCommand.Id,
                            updateCommand.Object.Name,
                            Description = updateCommand.Object.Description,
                            updateCommand.Object.Percentage,
                            updateCommand.Object.Value,
                            updateCommand.Object.MinOrderValue,
                            updateCommand.Object.MaxOrderValue,
                            updateCommand.Object.IsInactive,
                            updateCommand.Object.IsExclusive,
                            AppliedEventId = updateCommand.Object.AppliedEvent,
                            AppliedStartDate = updateCommand.Object.AppliedStartDate.Value,
                            AppliedEndDate = updateCommand.Object.AppliedEndDate == null ? (DateTime?)null : updateCommand.Object.AppliedEndDate.Value.Value,
                            StartEventId = updateCommand.Object.StartEvent,
                            StartEventEndDate = updateCommand.Object.StartEventEndDate == null ? (DateTime?)null : updateCommand.Object.StartEventEndDate.Value.Value,
                            updateCommand.Object.PeriodFromStartDays,
                            DiscountRewardTypeId = updateCommand.Object.RewardType,
                            DiscountEligibleProductsTypeId = updateCommand.Object.EligibleProductsType,
                            DiscountQualificationTypeId = updateCommand.Object.QualificationType
                        };

                        await connection.ExecuteAsync(sql, obj, transaction);

                        if (updateCommand.Object.DiscountedProducts != null && updateCommand.Object.DiscountedProducts.Count > 0)
                        {

                            var deleteCmd = "DELETE FROM [dbo].[DiscountedProducts] WHERE DiscountId = @Id";
                            await connection.ExecuteAsync(deleteCmd, new { updateCommand.Id }, transaction);
                            await CreateDiscountedProducts(updateCommand.Object.DiscountedProducts, updateCommand.Id, connection, transaction);
                        }

                        if (updateCommand.Object.QualifyingProducts != null && updateCommand.Object.QualifyingProducts.Count > 0)
                        {
                            var deleteCmd = "DELETE FROM [dbo].[DiscountQualifyingProducts] WHERE DiscountId = @Id";
                            await connection.ExecuteAsync(deleteCmd, new { updateCommand.Id }, transaction);
                            await CreateQualifyingProducts(updateCommand.Object.QualifyingProducts, updateCommand.Id, connection, transaction);
                        }

                        if (updateCommand.Object.QualifyingProductCategories != null && updateCommand.Object.QualifyingProductCategories.Count > 0)
                        {
                            var deleteCmd = "DELETE FROM [dbo].[DiscountQualifyingProductCategories] WHERE DiscountId = @Id";
                            await connection.ExecuteAsync(deleteCmd, new { updateCommand.Id }, transaction);
                            await CreateQualifyingCategoires(updateCommand.Object.QualifyingProductCategories, updateCommand.Id, connection, transaction);
                        }

                        if (updateCommand.Object.EligibleProductCategoryIds != null && updateCommand.Object.EligibleProductCategoryIds.Count > 0)
                        {
                            var deleteCmd = "DELETE FROM [dbo].[DiscountEligibleProductCategories] WHERE DiscountId = @Id";
                            await connection.ExecuteAsync(deleteCmd, new { updateCommand.Id }, transaction);
                            await CreateEligibleCategoires(updateCommand.Object.EligibleProductCategoryIds, updateCommand.Id, connection, transaction);
                        }

                        if (updateCommand.Object.DiscountCodes != null && updateCommand.Object.DiscountCodes.Count > 0)
                        {
                            foreach(var code in updateCommand.Object.DiscountCodes)
                            {
                                var c = new DiscountCode(Guid.NewGuid(), Guid.Parse(updateCommand.Id), code.Code, code.Description, code.IsInactive);
                                var codeSql = await SqlQueryReader.GetSqlQueryAsync("InsertDiscountCode.sql");
                                await connection.ExecuteAsync(codeSql, c, transaction);
                            }     
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                return await ExecuteAsync(new GetByIdQuery<Discount>(updateCommand.Id));
            }
        }

        

        public async Task<Discount> ExecuteAsync(CreateCommand<Discount> createCommand)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var discountId = Guid.NewGuid();

                        var discountSql = await SqlQueryReader.GetSqlQueryAsync("InsertDiscount.sql");

                        var obj =
                        new
                        {
                            Id = discountId,
                            createCommand.Object.Name,
                            createCommand.Object.Description,
                            createCommand.Object.Percentage,
                            createCommand.Object.Value,
                            createCommand.Object.MinOrderValue,
                            createCommand.Object.MaxOrderValue,
                            createCommand.Object.IsInactive,
                            createCommand.Object.IsExclusive,
                            AppliedEventId = createCommand.Object.AppliedEvent,
                            AppliedStartDate = createCommand.Object.AppliedStartDate.Value,
                            AppliedEndDate = createCommand.Object.AppliedEndDate == null ? (DateTime?)null : createCommand.Object.AppliedEndDate.Value.Value,
                            StartEventId = createCommand.Object.StartEvent,
                            StartEventEndDate = createCommand.Object.StartEventEndDate == null ? (DateTime?)null : createCommand.Object.StartEventEndDate.Value.Value,
                            PeriodFromStartDays = createCommand.Object.PeriodFromStartDays,
                            DiscountRewardTypeId = createCommand.Object.RewardType,
                            DiscountEligibleProductsTypeId = createCommand.Object.EligibleProductsType,
                            DiscountQualificationTypeId = createCommand.Object.QualificationType
                        };

                        await connection.ExecuteAsync(discountSql, obj, transaction);

                        if (createCommand.Object.DiscountedProducts != null && createCommand.Object.DiscountedProducts.Count > 0)
                            await CreateDiscountedProducts(createCommand.Object.DiscountedProducts, discountId.ToString(), connection, transaction);

                        if (createCommand.Object.QualifyingProducts != null && createCommand.Object.QualifyingProducts.Count > 0)
                            await CreateQualifyingProducts(createCommand.Object.QualifyingProducts, discountId.ToString(), connection, transaction);
                        if (createCommand.Object.QualifyingProductCategories != null && createCommand.Object.QualifyingProductCategories.Count > 0)
                            await CreateQualifyingCategoires(createCommand.Object.QualifyingProductCategories, discountId.ToString(), connection, transaction);
                        if (createCommand.Object.EligibleProductCategoryIds != null && createCommand.Object.EligibleProductCategoryIds.Count > 0)
                            await CreateEligibleCategoires(createCommand.Object.EligibleProductCategoryIds, discountId.ToString(), connection, transaction);
                        if (createCommand.Object.DiscountCodes != null && createCommand.Object.DiscountCodes.Count > 0)
                        {
                            foreach (var code in createCommand.Object.DiscountCodes)
                            {
                                var c = new DiscountCode(Guid.NewGuid(), discountId, code.Code, code.Description, code.IsInactive);
                                var codeSql = await SqlQueryReader.GetSqlQueryAsync("InsertDiscountCode.sql");
                                await connection.ExecuteAsync(codeSql, c, transaction);
                            }
                                
                        }

                        transaction.Commit();

                        return await ExecuteAsync(new GetByIdQuery<Discount>(discountId));
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static async Task CreateQualifyingProducts(IList<LineItem> products, string discountId, SqlConnection connection, SqlTransaction transaction)
        {
            var qualifyingProductSql = await SqlQueryReader.GetSqlQueryAsync("InsertDiscountQualifyingProduct.sql");
            foreach (var p in products)
                await connection.ExecuteAsync(qualifyingProductSql, new { p.ProductCode, discountId, p.Quantity }, transaction);
        }

        private static async Task CreateDiscountedProducts(IList<LineItem> products, string discountId, SqlConnection connection, SqlTransaction transaction)
        {
            var discountedProductSql = await SqlQueryReader.GetSqlQueryAsync("InsertDiscountedProduct.sql");
            foreach (var p in products)
                await connection.ExecuteAsync(discountedProductSql, new { p.ProductCode, discountId, p.Quantity, p.Price }, transaction);
        }

        private static async Task CreateQualifyingCategoires(IList<DiscountQualifyingCategory> categories, string discountId, SqlConnection connection, SqlTransaction transaction)
        {
            var qualifyingProductSql = await SqlQueryReader.GetSqlQueryAsync("InsertDiscountQualifyingProductCategory.sql");
            foreach (var category in categories)
                await connection.ExecuteAsync(qualifyingProductSql, new { discountId, productCategoryId = category.ProductCategoryId, category.ProductQuantity }, transaction);
        }

        private static async Task CreateEligibleCategoires(IList<Guid> ids, string discountId, SqlConnection connection, SqlTransaction transaction)
        {
            var qualifyingProductSql = await SqlQueryReader.GetSqlQueryAsync("InsertDiscountEligibleProductCategory.sql");
            foreach (var productCategoryId in ids)
                await connection.ExecuteAsync(qualifyingProductSql, new { discountId, productCategoryId }, transaction);
        }

        public async Task<bool> ExecuteAsync(CreateCommand<DiscountCode> createCommand)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("InsertDiscountCode.sql");

                await connection.ExecuteAsync(sql, createCommand.Object);

                return true;
            }
        }

        public async Task<Discount> ExecuteAsync(GetDiscountByCodeQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetDiscountsByCode.sql");

                return await connection.ReadJsonAsync<Discount>(sql, new { query.DiscountCode });
            }
        }

    }
}
