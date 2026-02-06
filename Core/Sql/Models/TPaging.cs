using System.Text.Json.Serialization;

namespace Core.Sql.Models;

/// <summary>
/// Interface for paging response results.
/// </summary>
public interface IBaseSqlPagingRes
{
    /// <summary>
    /// Current page index (1-based).
    /// </summary>
    int PageIndex { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    int PageSize { get; set; }

    /// <summary>
    /// Total number of records across all pages.
    /// </summary>
    long TotalRecords { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    /// Indicates if there is a previous page.
    /// </summary>
    bool HasPreviousPage { get; }

    /// <summary>
    /// Indicates if there is a next page.
    /// </summary>
    bool HasNextPage { get; }
}

/// <summary>
/// Generic paging result container.
/// Implements standard paging metadata with type-safe item collection.
/// </summary>
/// <typeparam name="T">The type of items in the paged result.</typeparam>
public class TPaging<T> : IBaseSqlPagingRes
{
    /// <summary>
    /// The items for the current page.
    /// </summary>
    [JsonPropertyName("items")]
    public IEnumerable<T> Items { get; set; } = [];

    /// <summary>
    /// Current page index (1-based).
    /// </summary>
    [JsonPropertyName("pageIndex")]
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = SqlConstants.DefaultPageSize;

    /// <summary>
    /// Total number of records across all pages.
    /// </summary>
    [JsonPropertyName("totalRecords")]
    public long TotalRecords { get; set; }

    /// <summary>
    /// Total number of pages calculated from TotalRecords and PageSize.
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;

    /// <summary>
    /// Indicates if there is a previous page.
    /// </summary>
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// Indicates if there is a next page.
    /// </summary>
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => PageIndex < TotalPages;

    /// <summary>
    /// The number of items in the current page.
    /// </summary>
    [JsonPropertyName("currentPageSize")]
    public int CurrentPageSize => Items?.Count() ?? 0;

    /// <summary>
    /// The starting record number for this page (1-based).
    /// </summary>
    [JsonPropertyName("startRecord")]
    public long StartRecord => TotalRecords > 0 ? ((PageIndex - 1) * PageSize) + 1 : 0;

    /// <summary>
    /// The ending record number for this page.
    /// </summary>
    [JsonPropertyName("endRecord")]
    public long EndRecord => Math.Min(PageIndex * PageSize, TotalRecords);

    /// <summary>
    /// Creates a new empty TPaging instance.
    /// </summary>
    public TPaging() { }

    /// <summary>
    /// Creates a new TPaging instance with specified values.
    /// </summary>
    public TPaging(IEnumerable<T> items, int pageIndex, int pageSize, long totalRecords)
    {
        Items = items ?? [];
        PageIndex = pageIndex > 0 ? pageIndex : 1;
        PageSize = pageSize > 0 ? Math.Min(pageSize, SqlConstants.MaxPageSize) : SqlConstants.DefaultPageSize;
        TotalRecords = totalRecords >= 0 ? totalRecords : 0;
    }

    #region Factory Methods

    /// <summary>
    /// Creates an empty paging result.
    /// </summary>
    public static TPaging<T> Empty(int pageIndex = 1, int pageSize = SqlConstants.DefaultPageSize)
        => new([], pageIndex, pageSize, 0);

    /// <summary>
    /// Creates a paging result from a full collection (client-side paging).
    /// </summary>
    public static TPaging<T> FromCollection(IEnumerable<T> allItems, int pageIndex, int pageSize)
    {
        var itemsList = allItems?.ToList() ?? [];
        var totalRecords = itemsList.Count;
        var pagedItems = itemsList
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize);

        return new TPaging<T>(pagedItems, pageIndex, pageSize, totalRecords);
    }

    /// <summary>
    /// Creates a paging result with pre-paged data (server-side paging).
    /// </summary>
    public static TPaging<T> FromPagedData(IEnumerable<T> pagedItems, int pageIndex, int pageSize, long totalRecords)
        => new(pagedItems, pageIndex, pageSize, totalRecords);

    #endregion

    /// <summary>
    /// Maps the items to a different type while preserving paging metadata.
    /// </summary>
    public TPaging<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return new TPaging<TResult>(
            Items.Select(mapper),
            PageIndex,
            PageSize,
            TotalRecords
        );
    }
}

/// <summary>
/// Extension methods for paging operations.
/// </summary>
public static class PagingExtensions
{
    /// <summary>
    /// Converts an IEnumerable to a paged result.
    /// </summary>
    public static TPaging<T> ToPagedResult<T>(this IEnumerable<T> items, int pageIndex, int pageSize, long totalRecords)
        => new(items, pageIndex, pageSize, totalRecords);

    /// <summary>
    /// Validates and normalizes paging parameters.
    /// </summary>
    public static (int pageIndex, int pageSize) NormalizePagingParams(int pageIndex, int pageSize)
    {
        pageIndex = pageIndex < 1 ? SqlConstants.DefaultPageIndex : pageIndex;
        pageSize = pageSize < 1 ? SqlConstants.DefaultPageSize : Math.Min(pageSize, SqlConstants.MaxPageSize);
        return (pageIndex, pageSize);
    }
}
