using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IPaymentsHandler :
        IDataOperationHandler<GetPaymentRequestsQuery, IList<PaymentRequest>>,
        IDataOperationHandler<GetDeliveryIdsForPaymentQuery, IList<Guid>>,
        IDataOperationHandler<GetDeliveryAndTrasactionIdsForRefundQuery, IList<(Guid, Guid)>>,
        IDataOperationHandler<CreateCommand<CardTransaction>, Guid>,
        IDataOperationHandler<UpdateCommand<CardTransaction>, bool>,
        IDataOperationHandler<CreateCommand<CardTransactionBatch>, Guid>,
        IDataOperationHandler<UpdateCommand<CardTransactionBatch>, bool>,
        IDataOperationHandler<GetCardTransactionsQuery, IList<CardTransaction>>,
        IDataOperationHandler<GetByIdQuery<CardTransactionBatch>, CardTransactionBatch>
    {
    }
}
