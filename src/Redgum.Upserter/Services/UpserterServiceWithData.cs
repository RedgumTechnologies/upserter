using System;
using System.Collections.Generic;

namespace Redgum.Upserter.Services;

/// <summary>
/// Base Upserter Service that uses existing and supplied data to insert, update, and delete items.
/// </summary>
/// <typeparam name="TExisting">The type of existing items, often a type from an Entity Framework DbContext</typeparam>
/// <typeparam name="TSupplied">The type of supplied items, often a model used in a Post method.</typeparam>
/// <typeparam name="TExtraData">The type of data that will be supplied alongside existing and supplied items. This is often a Parent item that needs to be bound as part of the insert.</typeparam>
public abstract class UpserterService<TExisting, TSupplied, TExtraData>
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
    public UpsertResult<TExisting, TSupplied> Upsert<TKey>(
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
    protected abstract void Insert(TSupplied suppliedItem, TExtraData data);

    /// <summary>
    /// Update Method. Override this method to update an existing item.
    /// </summary>
    /// <param name="existingItem">The existing item to be Updated</param>
    /// <param name="suppliedItem">The supplied item to use as source data for the Update</param>
    /// <param name="data">Extra data to go along with the update, like a Parent item</param>
    protected abstract void Update(TExisting existingItem, TSupplied suppliedItem, TExtraData data);

    /// <summary>
    /// Delete Method. Override this method to delete an existing item.
    /// </summary>
    /// <param name="existingItem">The existing item to be Deleted</param>
    /// <param name="data">Extra data to go along with the Delete, like a Parent item</param>
    protected abstract void Delete(TExisting existingItem, TExtraData data);
}