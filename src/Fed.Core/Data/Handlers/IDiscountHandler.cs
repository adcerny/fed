using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IDiscountsHandler :
        IDataOperationHandler<GetByIdQuery<Discount>, Discount>,
        IDataOperationHandler<GetByIdsQuery<Discount>, IList<Discount>>,
        IDataOperationHandler<GetAllQuery<Discount>, IList<Discount>>,
        IDataOperationHandler<GetDiscountsByCustomerQuery, IList<CustomerDiscount>>,
        IDataOperationHandler<CreateCommand<Discount>, Discount>,
        IDataOperationHandler<UpdateCommand<Discount>, Discount>,
        IDataOperationHandler<CreateCommand<CustomerDiscount>, bool>,
        IDataOperationHandler<CreateCommand<DiscountCode>, bool>,
        IDataOperationHandler<GetDiscountByCodeQuery, Discount>,
        IDataOperationHandler<GetDiscountsApplicableQuery, IList<Discount>>
    { }
}