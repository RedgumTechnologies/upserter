using System;
using System.Collections.Generic;

namespace Redgum.Upserter.Services;

/// <summary>
/// Base Upserter Service that uses existing and supplied data to insert, update, and delete items.
/// </summary>
/// <remarks>
/// Use this service when you need to return a result from the Insert, Update, and Delete methods. For example, when you need to analyse a Comment field and return a list of all the 'user mentions' in the Comment.
/// </remarks>
/// <typeparam name="TExisting">The type of existing items, often a type from an Entity Framework DbContext</typeparam>
/// <typeparam name="TSupplied">The type of supplied items, often a model used in a Post method.</typeparam>
/// <typeparam name="TExtraData">The type of data that will be supplied alongside existing and supplied items. This is often a Parent item that needs to be bound as part of the insert.</typeparam>
/// <typeparam name="TInsertResult">The type of the Result that the Insert method will return.</typeparam>
/// <typeparam name="TUpdateResult">The type of the Result that the Update method will return.</typeparam>
/// <typeparam name="TDeleteResult">The type of the Result that the Delete method will return.</typeparam>
public abstract class UpserterService<TExisting, TSupplied, TExtraData, TInsertResult, TUpdateResult, TDeleteResult>
{
    /// <summary>
    /// Upsert will call Insert, Update, and Delete methods as needed to synchronize the existing items with the supplied items.
    /// </summary>
    /// <typeparam name="TKey">The type of the key that will be used to match ExistingItems to SuppliedItems</typeparam>
    /// <param name="existingItems">Collection of existing items</param>
    /// <param name="suppliedItems">Collection of supplied items to be upserted</param>
    /// <param name="selectKeyForExistingItems">Func to select the key of an Existing Item</param>
    /// <param name="selectKeyForSuppliedItems">Func to select the key of a Supplied Item</param>
    /// <param name="data">Extra data to be supplied to the Insert, Update and Delete operations, often a Parent item or similar</param>
    /// <returns>A Result set that contains all items supplied to the Insert Action, all items supplied to the Update Action and all items supplied to the Delete Action</returns>
    /// <exception cref="ArgumentNullException">existingItems is null</exception>
    /// <exception cref="ArgumentNullException">suppliedItems is null</exception>
    /// <exception cref="ArgumentNullException">selectKeyForExistingItems is null</exception>
    /// <exception cref="ArgumentNullException">selectKeyForSuppliedItems is null</exception>
    public UpsertResult<TExisting, TSupplied, TInsertResult, TUpdateResult, TDeleteResult> Upsert<TKey>(
        IEnumerable<TExisting> existingItems,
        IEnumerable<TSupplied> suppliedItems,
        Func<TExisting, TKey> selectKeyForExistingItems,
        Func<TSupplied, TKey> selectKeyForSuppliedItems,
        TExtraData data
        )
    {
        if (existingItems == null) throw new ArgumentNullException(nameof(existingItems));
        if (suppliedItems == null) throw new ArgumentNullException(nameof(suppliedItems));
        if (selectKeyForExistingItems == null) throw new ArgumentNullException(nameof(selectKeyForExistingItems));
        if (selectKeyForSuppliedItems == null) throw new ArgumentNullException(nameof(selectKeyForSuppliedItems));

        return UpsertMethods.Upsert(
            existingItems: existingItems,
            suppliedItems: suppliedItems,
            selectKeyForExistingItems: selectKeyForExistingItems,
            selectKeyForSuppliedItems: selectKeyForSuppliedItems,
            insertAction: suppliedItem => Insert(suppliedItem, data),
            updateAction: (existingItem, suppliedItem) => Update(existingItem, suppliedItem, data),
            deleteAction: existingItem => Delete(existingItem, data)
            );
    }

    /// <summary>
    /// Insert Method. Override this method to insert a new item.
    /// </summary>
    /// <param name="suppliedItem">The supplied item to use as source data for the Insert</param>
    /// <param name="data">Extra data to go along with the insert, like a Parent item</param>
    /// <returns></returns>
    protected abstract TInsertResult Insert(TSupplied suppliedItem, TExtraData data);

    /// <summary>
    /// Update Method. Override this method to update an existing item.
    /// </summary>
    /// <param name="existingItem">The existing item to be Updated</param>
    /// <param name="suppliedItem">The supplied item to use as source data for the Update</param>
    /// <param name="data">Extra data to go along with the update, like a Parent item</param>
    /// <returns></returns>
    protected abstract TUpdateResult Update(TExisting existingItem, TSupplied suppliedItem, TExtraData data);

    /// <summary>
    /// Delete Method. Override this method to delete an existing item.
    /// </summary>
    /// <param name="existingItem">The existing item to be Deleted</param>
    /// <param name="data">Extra data to go along with the Delete, like a Parent item</param>
    /// <returns></returns>
    protected abstract TDeleteResult Delete(TExisting existingItem, TExtraData data);
}

/// <summary>
/// Base Upserter Service that uses existing and supplied data to insert, update, and delete items.
/// </summary>
/// <remarks>
/// Use this service when you need to return a result from the Insert, Update, and Delete methods. For example, when you need to analyse a Comment field and return a list of all the 'user mentions' in the Comment.
/// </remarks>
/// <typeparam name="TExisting">The type of existing items, often a type from an Entity Framework DbContext</typeparam>
/// <typeparam name="TSupplied">The type of supplied items, often a model used in a Post method.</typeparam>
/// <typeparam name="TExtraData">The type of data that will be supplied alongside existing and supplied items. This is often a Parent item that needs to be bound as part of the insert.</typeparam>
/// <typeparam name="TResult">The type of the Result that the Insert, Update and Delete methods will return.</typeparam>
public abstract class UpserterService<TExisting, TSupplied, TExtraData, TResult> : UpserterService<TExisting, TSupplied, TExtraData, TResult, TResult, TResult>
{
    // No need to override anything here
}
