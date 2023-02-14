using Company.API.ElasticSearch;
using Elasticsearch.Net;
using ElasticSearchQueryable.ElasticsearchNet;
using ElasticSearchQueryable.Mapping;
using ElasticSearchQueryable;
using Nest;
using ElasticQueryable.Api.Queries;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var signInConfiguration = builder.Configuration.GetSection("ElasticConfiguration").Get<ElasticConfiguration>();
builder.Services.AddSingleton(signInConfiguration);

builder.Services.AddSingleton(s =>
{
    var pool = new SingleNodeConnectionPool(new Uri(signInConfiguration.ElasticUri));
    var settings = new ConnectionSettings(pool).ApiKeyAuthentication(signInConfiguration.ElasticKeyId, signInConfiguration.ElasticKeyValue).EnableApiVersioningHeader();
    return new ElasticClient(settings);
});

builder.Services
    .AddSingleton(s => new ElasticContext(new ElasticNetConnection(s.GetService<ElasticClient>().LowLevel),
        new TrivialElasticMapping(), retryPolicy: new NoRetryPolicy()));

builder.Services
                .AddGraphQLServer()
                .AddQueryType(d => d.Name("Query"))
                    .AddTypeExtension<CompanyTypeExtension>()
                .AddFiltering<LocationFilteringConvention>()
                .AddProjections()
                .AddSorting()
                .AddElasticSearchPagingProviders(defaultProvider: true);


var app = builder.Build();

app.MapGraphQL();

app.Run();
