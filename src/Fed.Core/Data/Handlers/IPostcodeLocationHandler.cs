using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;

namespace Fed.Core.Data.Handlers
{
    public interface IPostcodeLocationHandler :
        IDataOperationHandler<GetPostcodeLocationQuery, PostcodeLocation>,
        IDataOperationHandler<CreateCommand<PostcodeLocation>, bool>,
        IDataOperationHandler<CreateCommand<PostcodeQuery>, bool>,
        IDataOperationHandler<CreateCommand<PostcodeContact>, bool>
    { }
}