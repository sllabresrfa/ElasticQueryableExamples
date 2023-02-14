using HotChocolate.Execution.Configuration;
using HotChocolate.Types.Pagination;

public static partial class ElasticSearchDataRequestBuilderExtensions
{
    /// <summary>
    /// Adds the ElasticSearch cursor and offset paging providers.
    /// </summary>
    /// <param name="builder">
    /// The GraphQL configuration builder.
    /// </param>
    /// <param name="providerName">
    /// The name which shall be used to refer to this registration.
    /// </param>
    /// <param name="defaultProvider">
    /// Defines if these providers shall be registered as default providers.
    /// </param>
    /// <returns>
    /// Returns the GraphQL configuration builder for further configuration chaining.
    /// </returns>
    public static IRequestExecutorBuilder AddElasticSearchPagingProviders(
        this IRequestExecutorBuilder builder,
        string providerName = "default",
        bool defaultProvider = false)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.AddCursorPagingProvider<QueryableCursorPagingProvider>(
            providerName,
            defaultProvider);

        builder.AddOffsetPagingProvider<ElasticSearchOffsetPagingProvider>(
            providerName,
            defaultProvider);

        return builder;
    }
}
