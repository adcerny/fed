using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Models;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IOrdersHandler :
        IDataOperationHandler<GetLastOrderQuery, Order>,
        IDataOperationHandler<GetOrdersQuery, IList<Order>>,
        IDataOperationHandler<CreateCommand<Order>, Guid>,
        IDataOperationHandler<CreateOrderFromRecurringOrderCommand, Guid>,
        IDataOperationHandler<GetByIdQuery<Order>, Order>,
        IDataOperationHandler<GetOrderSummaryQuery, IList<OrderSummary>>,
        IDataOperationHandler<UpdateOrderItemCommand, bool>,
        IDataOperationHandler<CreateCommand<OrderItem>, bool>,
        IDataOperationHandler<CreateCommand<OrderDiscount>, bool>,
        IDataOperationHandler<UpdateOrderDiscountCommand, bool>
    { }
}