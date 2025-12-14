using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class SuppliersService : ISuppliersService
    {
        private readonly ILogger _logger;
        private readonly ISuppliersHandler _suppliersHandler;
        private readonly IReportingHandler _reportingHandler;
        private readonly IBakeryMinimumOrderService _bakeryMinimumOrderService;

        public SuppliersService(
            ILogger logger,
            ISuppliersHandler suppliersHandler,
            IReportingHandler reportingHandler,
            IBakeryMinimumOrderService bakeryMinimumOrderService)
        {
            _logger =
                logger
                ?? throw new ArgumentNullException(nameof(logger));

            _suppliersHandler =
                suppliersHandler
                ?? throw new ArgumentNullException(nameof(suppliersHandler));

            _reportingHandler =
                reportingHandler
                ?? throw new ArgumentNullException(nameof(reportingHandler));

            _bakeryMinimumOrderService =
                bakeryMinimumOrderService
                ?? throw new ArgumentNullException(nameof(bakeryMinimumOrderService));
        }


        public async Task<IList<Supplier>> GetSuppliersAsync() =>
            await _suppliersHandler.ExecuteAsync(new GetAllQuery<Supplier>());

        public async Task<Supplier> GetSupplierAsync(int id) =>
            await _suppliersHandler.ExecuteAsync(new GetByIdQuery<Supplier>(id.ToString()));

        public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
        {
            var suppliers = await GetSuppliersAsync();
            var newid = suppliers?.Max(s => s.Id) + 1 ?? 1;
            var newSupplier = new Supplier(newid, supplier.Name);

            return await _suppliersHandler.ExecuteAsync(new CreateCommand<Supplier>(newSupplier));
        }

        public async Task<bool> UpdateSupplierAsync(int id, Supplier supplier) =>
            await _suppliersHandler.ExecuteAsync(new UpdateCommand<Supplier>(Guid.Empty, supplier));

        public Task<IList<SupplierProductQuantity>> GetConfirmedSupplierQuantitiesAsync(int supplierId, Date deliveryDate) =>
            _reportingHandler.GetReportAsync<SupplierProductQuantity>(
                "GetSupplierConfirmedOrdersPerDay",
                new { Date = deliveryDate.Value, SupplierId = supplierId });

        public async Task<IDictionary<Date, IList<SupplierProductQuantity>>> GetSupplierForecastAsync(
            int supplierId,
            Date toDate,
            bool excludeFedBuffer = false)
        {
            var supplierForecast = new Dictionary<Date, IList<SupplierProductQuantity>>();

            var supplier = (Suppliers)supplierId;

            // Forecast data is generated from recurring orders.
            // Because a recurring order can change at any point
            // in time, we cannot/shouldn't return forecast data
            // for dates in the past, because the actually generated
            // order might have had different items at that time:
            var tomorrow = Date.Create(DateTime.Today.AddDays(1));
            var fromDate = tomorrow;

            // Set the max forecast period to 14 days:
            var forecastDays = (toDate.Value - fromDate.Value).TotalDays;

            if (forecastDays > 14)
                toDate = fromDate.AddDays(14);

            var date = fromDate;

            while (toDate >= date)
            {
                // Get confirmed order quantities first if orders have been generated:
                var forecast =
                    await _reportingHandler.GetReportAsync<SupplierProductQuantity>(
                        "GetSupplierConfirmedOrdersPerDay",
                        new { Date = date.Value, SupplierId = supplierId });

                // If ordres are not generated yet then use the forecast data:
                if (forecast == null || forecast.Count == 0)
                {
                    forecast =
                        await _reportingHandler.GetReportAsync<SupplierProductQuantity>(
                            "GetSupplierForecastPerDay",
                            new { Date = date.Value, SupplierId = supplierId });

                    if (supplier == Suppliers.SevenSeeded && !excludeFedBuffer)
                    {
                        await _bakeryMinimumOrderService.TopUpBreadOrderIfNeeded(forecast);
                    }
                }

                supplierForecast.Add(date, forecast ?? new List<SupplierProductQuantity>());

                date = date.AddDays(1);
            }

            return supplierForecast;
        }
    }
}