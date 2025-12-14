using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Data.Handlers
{
    public interface IProductCategoriesHandler :
        IDataOperationHandler<GetByIdQuery<ProductCategory>, ProductCategory>,
        IDataOperationHandler<GetAllQuery<ProductCategory>, IList<ProductCategory>>,
        IDataOperationHandler<CreateCommand<ProductCategory>, bool>,
        IDataOperationHandler<UpdateCommand<ProductCategory>, bool>,
         IDataOperationHandler<GetByIdQuery<ProductCategoryProducts>, IList<ProductCategoryProducts>>,
        IDataOperationHandler<CreateCommand<ProductCategoryProducts>, bool>
    {
    }
}
