using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IProcessPaymentsService
    {
        Task<CardTransactionBatch> ProcessPayments(Date deliveryDate);
        Task<bool> UpdateCardTransactions(Guid batchId);
    }
}
