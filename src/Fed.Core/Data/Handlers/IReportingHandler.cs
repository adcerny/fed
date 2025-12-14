using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Data.Handlers
{
    public interface IReportingHandler
    {
        Task<IList<T>> GetReportAsync<T>(string reportName, object queryArgs = null);
        Task<IList<T>> GetNewCustomerSummaryAsync<T>();
    }
}