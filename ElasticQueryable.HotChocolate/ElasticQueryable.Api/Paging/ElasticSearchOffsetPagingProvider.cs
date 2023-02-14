using HotChocolate.Internal;
using HotChocolate.Types.Pagination;
using System.Reflection;

public class ElasticSearchOffsetPagingProvider
    : OffsetPagingProvider
{
    private static readonly MethodInfo _createHandler =
        typeof(ElasticSearchOffsetPagingProvider).GetMethod(
            nameof(CreateHandlerInternal),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    public override bool CanHandle(IExtendedType source)
    {
        return true;
    }

    protected override OffsetPagingHandler CreateHandler(
        IExtendedType source,
        PagingOptions options)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return (OffsetPagingHandler)_createHandler
            .MakeGenericMethod(source.ElementType?.Source ?? source.Source)
            .Invoke(null, new object[] { options })!;
    }

    private static ElasticSearchOffsetPagingHandler<TEntity> CreateHandlerInternal<TEntity>(
        PagingOptions options) =>
        new ElasticSearchOffsetPagingHandler<TEntity>(options);
}
