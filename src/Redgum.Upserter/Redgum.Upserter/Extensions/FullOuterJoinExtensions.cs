using System;
using System.Collections.Generic;
using System.Linq;

namespace Redgum.Upserter.Extensions;

// Concepts and code from https://stackoverflow.com/a/13503860/6042

/// <summary>
/// Extension methods to provide full outer join functionality.
/// </summary>
public static class FullOuterJoinExtensions
{
    /// <summary>
    /// Full outer join extension method.
    /// </summary>
    public static IEnumerable<TResult> FullOuterGroupJoin<TA, TB, TKey, TResult>(
        this IEnumerable<TA> a,
        IEnumerable<TB> b,
        Func<TA, TKey> selectKeyA,
        Func<TB, TKey> selectKeyB,
        Func<IEnumerable<TA>, IEnumerable<TB>, TKey, TResult> projection,
        IEqualityComparer<TKey>? cmp = null
        )
    {
        cmp ??= EqualityComparer<TKey>.Default;
        var aAsLookup = a.ToLookup(selectKeyA, cmp);
        var bAsLookup = b.ToLookup(selectKeyB, cmp);

        var keys = new HashSet<TKey>(aAsLookup.Select(p => p.Key), cmp);
        keys.UnionWith(bAsLookup.Select(p => p.Key));

        var join = from key in keys
                   let xa = aAsLookup[key]
                   let xb = bAsLookup[key]
                   select projection(xa, xb, key);

        return join;
    }

    /// <summary>
    /// Full outer join extension method.
    /// </summary>
    public static IEnumerable<TResult> FullOuterJoin<TA, TB, TKey, TResult>(
        this IEnumerable<TA> a,
        IEnumerable<TB> b,
        Func<TA, TKey> selectKeyA,
        Func<TB, TKey> selectKeyB,
        Func<TA, TB, TKey, TResult> projection,
        TA? defaultA = default,
        TB? defaultB = default,
        IEqualityComparer<TKey>? cmp = null
        )
    {
        cmp ??= EqualityComparer<TKey>.Default;
        var aAsLookup = a.ToLookup(selectKeyA, cmp);
        var bAsLookup = b.ToLookup(selectKeyB, cmp);

        var keys = new HashSet<TKey>(aAsLookup.Select(p => p.Key), cmp);
        keys.UnionWith(bAsLookup.Select(p => p.Key));

        var join = from key in keys
                   from xa in aAsLookup[key].DefaultIfEmpty(defaultA)
                   from xb in bAsLookup[key].DefaultIfEmpty(defaultB)
                   select projection(xa, xb, key);

        return join;
    }
}


