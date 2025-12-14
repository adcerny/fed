using System.Threading.Tasks;

namespace Fed.Core.Data
{
    public interface IDataOperationHandler<in TDataOperation, TResult>
    {
        Task<TResult> ExecuteAsync(TDataOperation operation);
    }
}