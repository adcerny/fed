using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface ICustomerMarketingAttributesHandler :
        IDataOperationHandler<GetByIdQuery<CustomerMarketingAttribute>, CustomerMarketingAttribute>,
        IDataOperationHandler<GetAllQuery<CustomerMarketingAttribute>, IList<CustomerMarketingAttribute>>,
        IDataOperationHandler<CreateCommand<CustomerMarketingAttribute>, bool>,
        IDataOperationHandler<UpdateCommand<CustomerMarketingAttribute>, bool>
    { }
}