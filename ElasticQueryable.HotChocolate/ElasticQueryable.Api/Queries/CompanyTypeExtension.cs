using ElasticQueryable.Api.Types;
using HotChocolate.Types.Pagination;

namespace ElasticQueryable.Api.Queries
{
    public class CompanyTypeExtension : ObjectTypeExtension<CompanyQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<CompanyQueries> descriptor)
        {
            descriptor
                .Name("Query");

            descriptor
                .Field(f => f.GetCompanies(default, default))
                .Type<ListType<CompanyType>>()
                .UseOffsetPaging<CompanyType>(options: new PagingOptions
                {
                    DefaultPageSize = 10,
                    MaxPageSize = 9999,
                    IncludeTotalCount = true
                })
                .UseProjection()
                .UseFiltering();
        }
    }
}
