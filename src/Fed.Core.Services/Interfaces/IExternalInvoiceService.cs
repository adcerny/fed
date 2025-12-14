using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IExternalInvoiceService
    {
        Task<byte[]> GetExternalInvoicePdf(string externalInvoiceId);

        Task<IList<Invoice>> GetInvoices(IEnumerable<Guid> InvoiceNumbers);
    }
}
