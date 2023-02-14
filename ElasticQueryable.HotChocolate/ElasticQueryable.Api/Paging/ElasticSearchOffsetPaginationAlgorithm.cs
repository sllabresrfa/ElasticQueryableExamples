using HotChocolate.Types.Pagination;
using System.Collections;

public abstract class ElasticSearchOffsetPaginationAlgorithm<TQuery, TEntity>
{
    /// <summary>
    /// Applies the pagination algorithm to the provided data.
    /// </summary>
    /// <param name="query">The query builder.</param>
    /// <param name="arguments">The paging arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public ValueTask<CollectionSegment<TEntity>> ApplyPaginationAsync(
        TQuery query,
        OffsetPagingArguments arguments,
        CancellationToken cancellationToken) =>
        ApplyPaginationAsync(query, arguments, null, cancellationToken);

    private const int maxResultWindow = 10000;

    /// <summary>
    /// Applies the pagination algorithm to the provided data.
    /// </summary>
    /// <param name="query">The query builder.</param>
    /// <param name="arguments">The paging arguments.</param>
    /// <param name="totalCount">Specify the total amount of elements</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async ValueTask<CollectionSegment<TEntity>> ApplyPaginationAsync(
        TQuery query,
        OffsetPagingArguments arguments,
        int? totalCount,
        CancellationToken cancellationToken)
    {
        Func<CancellationToken, ValueTask<int>> getTotalCount = totalCount is null
            ? async ct => await CountAsync(query, ct)
            : _ => new ValueTask<int>(totalCount.Value);

        TQuery sliced = query;


        if (arguments.Skip is { } skip)
        {
            sliced = ApplySkip(sliced, skip);
        }

        if (arguments.Take is { } take)
        {
            sliced = ApplyTake(sliced, take + 1);
        }

        IReadOnlyList<TEntity> items =
            await ExecuteAsync(sliced, cancellationToken).ConfigureAwait(false);

        skip = arguments.Skip ?? 0;

        bool hasNextPage = items.Count == arguments.Take + 1 && skip + arguments.Take < maxResultWindow;
        bool hasPreviousPage = skip > 0;

        CollectionSegmentInfo pageInfo = new(hasNextPage, hasPreviousPage);

        items = new SkipLastCollection<TEntity>(items, skipLast: hasNextPage);

        return new CollectionSegment<TEntity>(
            items,
            pageInfo,
            getTotalCount);
    }

    /// <summary>
    /// Override this method to apply a skip on top of the provided query.
    /// </summary>
    protected abstract TQuery ApplySkip(TQuery query, int skip);

    /// <summary>
    /// Override this method to apply a take (limit) on top of the provided query.
    /// </summary>
    protected abstract TQuery ApplyTake(TQuery query, int take);

    /// <summary>
    /// Override this to implement a count function on top of the provided query.
    /// </summary>
    protected abstract ValueTask<int> CountAsync(
        TQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Override this to implement the query execution.
    /// </summary>
    protected abstract ValueTask<IReadOnlyList<TEntity>> ExecuteAsync(
        TQuery query,
        CancellationToken cancellationToken);

    private static ValueTask<int> GetTotalCountAssert(CancellationToken _) =>
        throw new InvalidOperationException();

    private class SkipLastCollection<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _items;
        private readonly bool _skipLast;

        public SkipLastCollection(
            IReadOnlyList<T> items,
            bool skipLast = false)
        {
            _items = items;
            _skipLast = skipLast;
            Count = _items.Count;

            if (skipLast && Count > 0)
            {
                Count--;
            }
        }

        public int Count { get; }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (i == _items.Count - 1 && _skipLast)
                {
                    break;
                }

                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T this[int index] => _items[index];
    }
}
