using ElasticSearchQueryable;

namespace ElasticQueryable.Api.Queries
{
    public class CompanyQueries
    {
        public IQueryable<ElasticQueryable.Api.Types.Company> GetCompanies([Service] ElasticContext elasticSearchContext, [Service] ElasticConfiguration configuration)
         => elasticSearchContext.Query<ElasticQueryable.Api.Types.Company>(configuration.ElasticIndex);
    }
}
