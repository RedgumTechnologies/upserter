using Redgum.Upserter.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redgum.Upserter;

/// <summary>
/// Extension methods to assist with upserting data
/// </summary>
public static class UpsertMethods
{
    /// <summary>
    /// Extension method to assist with writing Upsert logic
    /// </summary>
    /// <typeparam name="TExisting">The type of Existing items, often a type from an Entity Framework DbContext</typeparam>
    /// <typeparam name="TSupplied">The type of Supplied items, often a model used in a Post method.</typeparam>
    /// <typeparam name="TKey">The type of the key that will be used to match ExistingItems to SuppliedItems</typeparam>
    /// <param name="existingItems">Collection of existing items</param>
    /// <param name="suppliedItems">Collection of supplied items to be upserted</param>
    /// <param name="selectKeyForExistingItems">Func to select the key of an Existing Item</param>
    /// <param name="selectKeyForSuppliedItems">Func to select the key of a Supplied Item</param>
    /// <param name="insertAction">Action to be called once for each item that needs to be Inserted - Supplied Items that don't exist in the Existing Items set</param>
    /// <param name="updateAction">Action to be called once for each item that needs to be Updated - Supplied Items and Existing Items that have a matching Key</param>
    /// <param name="deleteAction">Action to be called once for each item that needs to be Deleted - Existing Items that don't exist in the Supplied Items set</param>
    /// <returns>A Result set that contains all items supplied to the Insert Action, all items supplied to the Update Action and all items supplied to the Delete Action</returns>
    /// <exception cref="ArgumentNullException">existingItems is null</exception>
    /// <exception cref="ArgumentNullException">suppliedItems is null</exception>
    /// <exception cref="ArgumentNullException">selectKeyForExistingItems is null</exception>
    /// <exception cref="ArgumentNullException">selectKeyForSuppliedItems is null</exception>
    /// <exception cref="ArgumentNullException">insertAction is null</exception>
    /// <exception cref="ArgumentNullException">updateAction is null</exception>
    /// <exception cref="ArgumentNullException">deleteAction is null</exception>
    public static UpsertResult<TExisting, TSupplied> Upsert<TSupplied, TExisting, TKey>(
        IEnumerable<TExisting> existingItems,
        IEnumerable<TSupplied> suppliedItems,
        Func<TExisting, TKey> selectKeyForExistingItems,
        Func<TSupplied, TKey> selectKeyForSuppliedItems,
        Action<TSupplied> insertAction,
        Action<TExisting, TSupplied> updateAction,
        Action<TExisting> deleteAction
        )
    {
        if (existingItems == null) throw new ArgumentNullException(nameof(existingItems));
        if (suppliedItems == null) throw new ArgumentNullException(nameof(suppliedItems));
        if (selectKeyForExistingItems == null) throw new ArgumentNullException(nameof(selectKeyForExistingItems));
        if (selectKeyForSuppliedItems == null) throw new ArgumentNullException(nameof(selectKeyForSuppliedItems));
        if (insertAction == null) throw new ArgumentNullException(nameof(insertAction));
        if (updateAction == null) throw new ArgumentNullException(nameof(updateAction));
        if (deleteAction == null) throw new ArgumentNullException(nameof(deleteAction));

        return Upsert(
            existingItems,
            suppliedItems,
            selectKeyForExistingItems,
            selectKeyForSuppliedItems,
            (insertSuppliedItems) => insertSuppliedItems.ToList().ForEach(insertAction),
            (updateItems) => updateItems.ToList().ForEach(i => updateAction(i.ExistingItem, i.SuppliedItem)),
            (deleteExistingItems) => deleteExistingItems.ToList().ForEach(deleteAction)
            );
    }

    /// <summary>
    /// Extension method to assist with writing Upsert logic
    /// </summary>
    /// <typeparam name="TExisting">The type of Existing items, often a type from an Entity Framework DbContext</typeparam>
    /// <typeparam name="TSupplied">The type of Supplied items, often a model used in a Post method.</typeparam>
    /// <typeparam name="TKey">The type of the key that will be used to match ExistingItems to SuppliedItems</typeparam>
    /// <param name="existingItems">Collection of existing items</param>
    /// <param name="suppliedItems">Collection of supplied items to be upserted</param>
    /// <param name="selectKeyForExistingItems">Func to select the key of an Existing Item</param>
    /// <param name="selectKeyForSuppliedItems">Func to select the key of a Supplied Item</param>
    /// <param name="insertAction">Action to be called once with all items that need to be Inserted - Supplied Items that don't exist in the Existing Items set</param>
    /// <param name="updateAction">Action to be called once with all items that need to be Updated - Supplied Items and Existing Items that have a matching Key</param>
    /// <param name="deleteAction">Action to be called once with all items that need to be Deleted - Existing Items that don't exist in the Supplied Items set</param>
    /// <returns>A Result set that contains all items supplied to the Insert Action, all items supplied to the Update Action and all items supplied to the Delete Action</returns>
    /// <exception cref="ArgumentNullException">existingItems is null</exception>
    /// <exception cref="ArgumentNullException">suppliedItems is null</exception>
    /// <exception cref="ArgumentNullException">selectKeyForExistingItems is null</exception>
    /// <exception cref="ArgumentNullException">selectKeyForSuppliedItems is null</exception>
    /// <exception cref="ArgumentNullException">insertAction is null</exception>
    /// <exception cref="ArgumentNullException">updateAction is null</exception>
    /// <exception cref="ArgumentNullException">deleteAction is null</exception>
    public static UpsertResult<TExisting, TSupplied> Upsert<TSupplied, TExisting, TKey>(
        IEnumerable<TExisting> existingItems,
        IEnumerable<TSupplied> suppliedItems,
        Func<TExisting, TKey> selectKeyForExistingItems,
        Func<TSupplied, TKey> selectKeyForSuppliedItems,
        Action<IEnumerable<TSupplied>> insertAction,
        Action<IEnumerable<MatchedItemPair<TExisting, TSupplied>>> updateAction,
        Action<IEnumerable<TExisting>> deleteAction
        )
    {
        if (existingItems == null) throw new ArgumentNullException(nameof(existingItems));
        if (suppliedItems == null) throw new ArgumentNullException(nameof(suppliedItems));
        if (selectKeyForExistingItems == null) throw new ArgumentNullException(nameof(selectKeyForExistingItems));
        if (selectKeyForSuppliedItems == null) throw new ArgumentNullException(nameof(selectKeyForSuppliedItems));
        if (insertAction == null) throw new ArgumentNullException(nameof(insertAction));
        if (updateAction == null) throw new ArgumentNullException(nameof(updateAction));
        if (deleteAction == null) throw new ArgumentNullException(nameof(deleteAction));

        var joinedResultsQuery = suppliedItems
            .FullOuterJoin(
                existingItems,
                selectKeyForSuppliedItems,
                selectKeyForExistingItems,
                (suppliedItem, existingItem, key) => new { suppliedItem, existingItem, key }
                );

        // we want to make sure we don't enumerate the query multiple times
        var joinedResultsSnapshot = joinedResultsQuery.ToArray();

        // and we want to make sure we don't enumerate the results through the Insert, Update, and Delete functions multiple times, so we ToList those
        var result = new UpsertResult<TExisting, TSupplied>
        {
            UnmatchedExistingItems = joinedResultsSnapshot.Where(i => i.suppliedItem == null).Select(i => i.existingItem).ToList(),
            UnmatchedSuppliedItems = joinedResultsSnapshot.Where(i => i.existingItem == null).Select(i => i.suppliedItem).ToList(),
            MatchedItems = joinedResultsSnapshot.Where(i => i.existingItem != null && i.suppliedItem != null).Select(i => new MatchedItemPair<TExisting, TSupplied>(i.existingItem, i.suppliedItem)).ToList()
        };

        insertAction(result.UnmatchedSuppliedItems);
        updateAction(result.MatchedItems);
        deleteAction(result.UnmatchedExistingItems);

        return result;
    }


    /// <summary>
    /// Extension method to assist with writing Upsert logic
    /// </summary>
    /// <typeparam name="TExisting">The type of Existing items, often a type from an Entity Framework DbContext</typeparam>
    /// <typeparam name="TSupplied">The type of Supplied items, often a model used in a Post method.</typeparam>
    /// <typeparam name="TKey">The type of the key that will be used to match ExistingItems to SuppliedItems</typeparam>
    /// <typeparam name="TInsertResult">The type of the Result that the Insert method will return.</typeparam>
    /// <typeparam name="TUpdateResult">The type of the Result that the Update method will return.</typeparam>
    /// <typeparam name="TDeleteResult">The type of the Result that the Delete method will return.</typeparam>
    /// <param name="existingItems">Collection of existing items</param>
    /// <param name="suppliedItems">Collection of supplied items to be upserted</param>
    /// <param name="selectKeyForExistingItems">Func to select the key of an Existing Item</param>
    /// <param name="selectKeyForSuppliedItems">Func to select the key of a Supplied Item</param>
    /// <param name="insertAction">Action to be called once for each item that needs to be Inserted - Supplied Items that don't exist in the Existing Items set</param>
    /// <param name="updateAction">Action to be called once for each item that needs to be Updated - Supplied Items and Existing Items that have a matching Key</param>
    /// <param name="deleteAction">Action to be called once for each item that needs to be Deleted - Existing Items that don't exist in the Supplied Items set</param>
    /// <returns>A Result set that contains all items supplied to the Insert Action, all items supplied to the Update Action and all items supplied to the Delete Action</returns>
    /// <exception cref="ArgumentNullException">existingItems is null</exception>
    /// <exception cref="ArgumentNullException">suppliedItems is null</exception>
    /// <exception cref="ArgumentNullException">selectKeyForExistingItems is null</exception>
    /// <exception cref="ArgumentNullException">selectKeyForSuppliedItems is null</exception>
    /// <exception cref="ArgumentNullException">insertAction is null</exception>
    /// <exception cref="ArgumentNullException">updateAction is null</exception>
    /// <exception cref="ArgumentNullException">deleteAction is null</exception>
    public static UpsertResult<TExisting, TSupplied, TInsertResult, TUpdateResult, TDeleteResult> Upsert<TSupplied, TExisting, TKey, TInsertResult, TUpdateResult, TDeleteResult>(
        IEnumerable<TExisting> existingItems,
        IEnumerable<TSupplied> suppliedItems,
        Func<TExisting, TKey> selectKeyForExistingItems,
        Func<TSupplied, TKey> selectKeyForSuppliedItems,
        Func<TSupplied, TInsertResult> insertAction,
        Func<TExisting, TSupplied, TUpdateResult> updateAction,
        Func<TExisting, TDeleteResult> deleteAction
        )
    {
        if (existingItems == null) throw new ArgumentNullException(nameof(existingItems));
        if (suppliedItems == null) throw new ArgumentNullException(nameof(suppliedItems));
        if (selectKeyForExistingItems == null) throw new ArgumentNullException(nameof(selectKeyForExistingItems));
        if (selectKeyForSuppliedItems == null) throw new ArgumentNullException(nameof(selectKeyForSuppliedItems));
        if (insertAction == null) throw new ArgumentNullException(nameof(insertAction));
        if (updateAction == null) throw new ArgumentNullException(nameof(updateAction));
        if (deleteAction == null) throw new ArgumentNullException(nameof(deleteAction));

        var joinedResultsQuery = suppliedItems
            .FullOuterJoin(
                existingItems,
                selectKeyForSuppliedItems,
                selectKeyForExistingItems,
                (suppliedItem, existingItem, key) => new { suppliedItem, existingItem, key }
            );

        // we want to make sure we don't enumerate the query multiple times
        var joinedResultsSnapshot = joinedResultsQuery.ToArray();

        // and we want to make sure we don't enumerate the results through the Insert, Update, and Delete functions multiple times, so we ToList those
        var result = new UpsertResult<TExisting, TSupplied, TInsertResult, TUpdateResult, TDeleteResult>
        {
            UnmatchedSuppliedItems = joinedResultsQuery
                .Where(i => i.existingItem == null)
                .Select(i => new UnmatchedSuppliedItemWithResult<TSupplied, TInsertResult>(i.suppliedItem, insertAction(i.suppliedItem)))
                .ToList(),
            MatchedItems = joinedResultsQuery
                .Where(i => i.existingItem != null && i.suppliedItem != null)
                .Select(i => new MatchedItemsWithResult<TExisting, TSupplied, TUpdateResult>(i.existingItem, i.suppliedItem, updateAction(i.existingItem, i.suppliedItem)))
                .ToList(),
            UnmatchedExistingItems = joinedResultsQuery
                .Where(i => i.suppliedItem == null)
                .Select(i => new UnmatchedExistingItemWithResult<TExisting, TDeleteResult>(i.existingItem, deleteAction(i.existingItem)))
                .ToList()
        };

        return result;
    }
}

/// <summary>
/// Matched pair of items from the existing and supplied collections
/// </summary>
/// <typeparam name="TExisting">Type of the Existing Item</typeparam>
/// <typeparam name="TSupplied">Type of the Supplied Item</typeparam>
/// <param name="existingItem">The Existing Item</param>
/// <param name="suppliedItem">The Supplied Item</param>
public class MatchedItemPair<TExisting, TSupplied>(TExisting existingItem, TSupplied suppliedItem)
{
    /// <summary>
    /// The Existing Item
    /// </summary>
    public TExisting ExistingItem { get; } = existingItem ?? throw new ArgumentNullException(nameof(existingItem));

    /// <summary>
    /// The Supplied Item
    /// </summary>
    public TSupplied SuppliedItem { get; } = suppliedItem ?? throw new ArgumentNullException(nameof(suppliedItem));
}

/// <summary>
/// Result of an Upsert operation
/// </summary>
/// <typeparam name="TExisting">Type of the Existing Item</typeparam>
/// <typeparam name="TSupplied">Type of the Supplied Item</typeparam>
public class UpsertResult<TExisting, TSupplied>
{
    /// <summary>
    /// Unmatched Existing Items. <br/>
    /// Items that exist in the Existing Items collection but not in the Supplied Items collection
    /// </summary>
    public List<TExisting> UnmatchedExistingItems { get; set; } = default!;

    /// <summary>
    /// Matched Items. <br/>
    /// Items that exist in both the Existing Items collection and the Supplied Items collection
    /// </summary>
    public List<MatchedItemPair<TExisting, TSupplied>> MatchedItems { get; set; } = default!;

    /// <summary>
    /// Unmatched Supplied Items. <br/>
    /// Items that exist in the Supplied Items collection but not in the Existing Items collection
    /// </summary>
    public List<TSupplied> UnmatchedSuppliedItems { get; set; } = default!;
}

/// <summary>
/// Result of an Upsert operation with Insert, Update, and Delete results
/// </summary>
/// <typeparam name="TExisting">Type of the Existing Item</typeparam>
/// <typeparam name="TSupplied">Type of the Supplied Item</typeparam>
/// <typeparam name="TInsertResult">The type of the Result that the Insert method has returned.</typeparam>
/// <typeparam name="TUpdateResult">The type of the Result that the Update method has returned.</typeparam>
/// <typeparam name="TDeleteResult">The type of the Result that the Delete method has returned.</typeparam>
public class UpsertResult<TExisting, TSupplied, TInsertResult, TUpdateResult, TDeleteResult>
{
    /// <summary>
    /// Unmatched Supplied Items. <br/>
    /// Items that exist in the Supplied Items collection but not in the Existing Items collection
    /// </summary>
    public List<UnmatchedSuppliedItemWithResult<TSupplied, TInsertResult>> UnmatchedSuppliedItems { get; set; } = default!;

    /// <summary>
    /// Matched Items. <br/>
    /// Items that exist in both the Existing Items collection and the Supplied Items collection
    /// </summary>
    public List<MatchedItemsWithResult<TExisting, TSupplied, TUpdateResult>> MatchedItems { get; set; } = default!;

    /// <summary>
    /// Unmatched Existing Items. <br/>
    /// Items that exist in the Existing Items collection but not in the Supplied Items collection
    /// </summary>
    public List<UnmatchedExistingItemWithResult<TExisting, TDeleteResult>> UnmatchedExistingItems { get; set; } = default!;
}

/// <summary>
/// Unmatched Supplied Item with the result of the Insert operation
/// </summary>
/// <typeparam name="TSupplied">Type of the Supplied Item</typeparam>
/// <typeparam name="TInsertResult">The type of the Result that the Insert method has returned.</typeparam>
/// <param name="suppliedItem">The Supplied Item </param>
/// <param name="insertResult">The result from the Insert operation</param>
public class UnmatchedSuppliedItemWithResult<TSupplied, TInsertResult>(TSupplied suppliedItem, TInsertResult? insertResult)
{
    /// <summary>
    /// The supplied item
    /// </summary>
    public TSupplied SuppliedItem { get; } = suppliedItem ?? throw new ArgumentNullException(nameof(suppliedItem));

    /// <summary>
    /// The result from the Insert operation
    /// </summary>
    public TInsertResult? InsertResult { get; } = insertResult;
}

/// <summary>
/// Matched pair of items from the Existing and Supplied collections with the result of the Update operation
/// </summary>
/// <typeparam name="TExisting">Type of the Existing Item</typeparam>
/// <typeparam name="TSupplied">Type of the Supplied Item</typeparam>
/// <typeparam name="TUpdateResult">Type of the Result of the Update operation</typeparam>
/// <param name="existingItem">The Existing Item</param>
/// <param name="suppliedItem">The Supplied Item</param>
/// <param name="updateResult">The result from the Update operation</param>
public class MatchedItemsWithResult<TExisting, TSupplied, TUpdateResult>(TExisting existingItem, TSupplied suppliedItem, TUpdateResult updateResult)
{
    /// <summary>
    /// The Existing Item
    /// </summary>
    public TExisting ExistingItem { get; } = existingItem ?? throw new ArgumentNullException(nameof(existingItem));

    /// <summary>
    /// The Supplied Item
    /// </summary>
    public TSupplied SuppliedItem { get; } = suppliedItem ?? throw new ArgumentNullException(nameof(suppliedItem));
    
    /// <summary>
    /// The result from the Update operation
    /// </summary>
    public TUpdateResult? UpdateResult { get; } = updateResult;
}

/// <summary>
/// Unmatched Existing Item with the result of the Delete operation
/// </summary>
/// <typeparam name="TExisting">Type of Existing Item</typeparam>
/// <typeparam name="TDeleteResult">Type of the Result of the Delete operation</typeparam>
/// <param name="existingItem">The Existing Item </param>
/// <param name="deleteResult">The result from the Delete operation</param>
public class UnmatchedExistingItemWithResult<TExisting, TDeleteResult>(TExisting existingItem, TDeleteResult? deleteResult)
{
    /// <summary>
    /// The Existing Item
    /// </summary>
    public TExisting ExistingItem { get; } = existingItem ?? throw new ArgumentNullException(nameof(existingItem));

    /// <summary>
    /// The result from the Delete operation
    /// </summary>
    public TDeleteResult? DeleteResult { get; } = deleteResult;
}