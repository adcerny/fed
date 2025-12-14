using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface ICustomersHandler :
        IDataOperationHandler<GetByIdQuery<Customer>, Customer>,
        IDataOperationHandler<GetByIdQuery<FullCustomerInfo>, FullCustomerInfo>,
        IDataOperationHandler<GetCustomersQuery, IList<Customer>>,
        IDataOperationHandler<CreateCommand<Customer>, Customer>,
        IDataOperationHandler<UpdateCommand<Customer>, bool>,
        IDataOperationHandler<GetCustomerByContactIdQuery, Customer>,
        IDataOperationHandler<GetCustomerIdByEmailQuery, Guid?>
    { }
}