// ---------------------------------------------------------------------------
// <copyright file="BookingsContainerExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Bookings.Client
{
    using System.Collections.Generic;

    using Microsoft.OData.Client;

    public static class BookingsContainerExtensions
    {
        /// <summary>
        /// Creates a data object with change tracking
        /// that can be used to POST a new entity with
        /// just the data that was set or modified in the data object.
        /// </summary>
        /// <typeparam name="T">Type of the entity to create.</typeparam>
        /// <param name="entitySet">The entity set where the entity lives.</param>
        /// <returns>
        /// A data object with change tracking enabled.
        /// </returns>
        /// <remarks>
        /// Operation is persisted when <see cref="DataServiceContext.SaveChangesAsync(SaveChangesOptions)"/> is invoked
        /// with <see cref="SaveChangesOptions.PostOnlySetProperties"/>.
        /// </remarks>
        public static T NewEntityWithChangeTracking<T>(this DataServiceQuery<T> entitySet)
            where T : new()
        {
            var entitySetName = entitySet.Context.BaseUri.MakeRelativeUri(entitySet.RequestUri).ToString();
            var changeTrackingCollection = new DataServiceCollection<T>(entitySet.Context, new List<T>(), TrackingMode.AutoChangeTracking, entitySetName, null, null);
            var entity = new T();
            changeTrackingCollection.Add(entity);
            return entity;
        }

        /// <summary>
        /// Creates a data object with change tracking
        /// that can be used to PATCH an existing entity with
        /// just the data that was set or modified in the data object.
        /// </summary>
        /// <typeparam name="T">Type of the entity to create.</typeparam>
        /// <param name="entity">The entity to be patched.</param>
        /// <returns>
        /// A data object with change tracking enabled.
        /// </returns>
        /// <remarks>
        /// Operation is persisted when <see cref="DataServiceContext.SaveChangesAsync()"/> is invoked.
        /// </remarks>
        public static T PatchEntityWithChangeTracking<T>(this DataServiceQuerySingle<T> entity)
        {
            return new DataServiceCollection<T>(entity)[0];
        }
    }
}