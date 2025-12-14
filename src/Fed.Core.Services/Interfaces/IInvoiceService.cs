using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<IList<Invoice>> GenerateInvoices(DateRange dateRange);
        Task<Invoice> GetInvoice(Guid Id);
        Task<byte[]> GetInvoicePdf(Guid Id);
        Task<IList<Invoice>> GetInvoices(Guid ContactId);
        Task<Guid> CreateInvoice(Invoice invoice);
        Task<bool> UpdateInvoice(Invoice invoice);
    }
}