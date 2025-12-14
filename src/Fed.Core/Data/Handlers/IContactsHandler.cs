using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IContactsHandler :
        IDataOperationHandler<GetByIdQuery<Contact>, Contact>,
        IDataOperationHandler<GetContactsQuery, IList<Contact>>,
        IDataOperationHandler<CreateContactCommand, Guid>,
        IDataOperationHandler<UpdateCommand<Contact>, bool>,
        IDataOperationHandler<DeleteCommand<Contact>, bool>,
        IDataOperationHandler<UpdateMarketingConsentCommand, bool>
    { }
}