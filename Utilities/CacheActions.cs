using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using test2.Models;

namespace test2.Utilities
{
    public static class CacheActions
    {
        private static readonly object _locker = new object();
        private static readonly int _defaultExpirationInSeconds = 0;
        private static readonly string _entityName = "Patient";
        private static readonly string _foreignEntityName = "LabResults";

        public static IEnumerable<Patient> SearchLabResults(IMemoryCache memoryCache, LabSearch lab)
        {
            List<LabResults> list = new List<LabResults>();
            List<Patient> patients = new List<Patient>();
            List<Patient> resultPatients = new List<Patient>();

            // Get the entryies
            System.Reflection.PropertyInfo cacheEntriesCollectionDefinition = typeof(MemoryCache).GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Populate the definition with your IMemoryCache instance.  
            // It needs to be cast as a dynamic, otherwise you can't
            // loop through it due to it being a collection of objects.
            dynamic cacheEntriesCollection = cacheEntriesCollectionDefinition.GetValue(memoryCache) as dynamic;

            // Define a new list we'll be adding the cache entries too
            List<Microsoft.Extensions.Caching.Memory.ICacheEntry> cacheCollectionValues = new List<Microsoft.Extensions.Caching.Memory.ICacheEntry>();

            foreach (dynamic cacheItem in cacheEntriesCollection)
            {
                // Get the "Value" from the key/value pair which contains the cache entry   
                ICacheEntry cacheItemValue = cacheItem.GetType().GetProperty("Value").GetValue(cacheItem, null);

                // Add the cache entry to the list
                cacheCollectionValues.Add(cacheItemValue);
            }


            foreach (object item in cacheCollectionValues.Select(i => i.Value))
            {
                if (item.GetType().Name == _foreignEntityName) list.Add(item as LabResults);
                if (item.GetType().Name == _entityName) patients.Add(item as Patient);
            }

            //list of all patient ids in lab results
            IEnumerable<Guid> results = list.Where(i => i.LabType.ToUpper() == lab.LabType.ToUpper() && Convert.ToDateTime(i.EnteredTime) <= Convert.ToDateTime(lab.ToDate) && Convert.ToDateTime(i.TestTime) >= Convert.ToDateTime(lab.FromDate)).Select(i => i.PatientID);
            List<Patient> test = patients.Where(i => results.Contains(i.PatientId)).ToList();
            return test;
        }
        /// <summary>
        /// Get an item in the cache
        /// </summary>
        /// <param name="memoryCache"></param>
        /// <returns></returns>
        public static List<ICacheEntry> GetItems<T>(IMemoryCache memoryCache)
        {
            lock (_locker)
            {
                // Get the entryies
                var cacheEntriesCollectionDefinition = typeof(MemoryCache).GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                // Populate the definition with your IMemoryCache instance.  
                // It needs to be cast as a dynamic, otherwise you can't
                // loop through it due to it being a collection of objects.
                var cacheEntriesCollection = cacheEntriesCollectionDefinition.GetValue(memoryCache) as dynamic;

                //var values = memoryCache.GetValues<ICacheEntry>().Where(c => c.Value is T).Select(c => (T)c.Value).ToList();
                // Define a new list we'll be adding the cache entries too
                List<Microsoft.Extensions.Caching.Memory.ICacheEntry> cacheCollectionValues = new List<Microsoft.Extensions.Caching.Memory.ICacheEntry>();

                foreach (var cacheItem in cacheEntriesCollection)
                {
                    // Get the "Value" from the key/value pair which contains the cache entry   
                    ICacheEntry cacheItemValue = cacheItem.GetType().GetProperty("Value").GetValue(cacheItem, null);

                    // Add the cache entry to the list
                    cacheCollectionValues.Add(cacheItemValue);
                }
                return cacheCollectionValues;
            }
        }

        /// <summary>
        /// Sets an item in the cache and set the expiration of the cache item 
        /// </summary>
        /// <param name="memoryCache"></param>
        /// <param name="key"></param>
        /// <param name="itemToCache"></param>
        /// <returns></returns>
        public static string AddItem<T>(IMemoryCache memoryCache, Guid key, T itemToCache)
        {
            try
            {

                lock (_locker)
                {
                    if (!memoryCache.TryGetValue(key, out T existingItem))
                    {
                        var cts = new CancellationTokenSource(_defaultExpirationInSeconds > 0 ? _defaultExpirationInSeconds * 1000 : -1);
                        var cacheEntryOptions = new MemoryCacheEntryOptions().AddExpirationToken(new CancellationChangeToken(cts.Token));

                        memoryCache.Set(key, itemToCache, cacheEntryOptions);
                        return key.ToString();
                    }
                }
                return null; //Item not added, the key already exists
            }
            catch (Exception err)
            {
                return err.Message.ToString();
            }
        }

        /// <summary>
        /// Retrieves a cache item. Possible to set the expiration of the cache item in seconds. 
        /// </summary>
        /// <param name="memoryCache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetItem<T>(IMemoryCache memoryCache, Guid key)
        {
            try
            {
                lock (_locker)
                {
                    if (memoryCache.TryGetValue(key, out T cachedItem))
                    {
                        return cachedItem;
                    }
                }
                return default(T);

            }
            catch (Exception err)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Sets an item in the cache and set the expiration of the cache item 
        /// </summary>
        /// <param name="memoryCache"></param>
        /// <param name="key"></param>
        /// <param name="itemToCache"></param>
        /// <returns></returns>
        public static bool SetItem<T>(IMemoryCache memoryCache, Guid key, T itemToCache)
        {
            try
            {

                lock (_locker)
                {
                    if (GetItem<T>(memoryCache, key) != null)
                    {
                        AddItem<T>(memoryCache, key, itemToCache);
                        return true;
                    }
                    UpdateItem(memoryCache, key, itemToCache);
                }
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }


        /// <summary>
        /// Updates an item in the cache and set the expiration of the cache item 
        /// </summary>
        /// <param name="memoryCache"></param>
        /// <param name="key"></param>
        /// <param name="itemToCache"></param>
        /// <returns></returns>
        public static string UpdateItem<T>(IMemoryCache memoryCache, Guid key, T itemToCache)
        {

            lock (_locker)
            {
                T existingItem = GetItem<T>(memoryCache, key);
                if (existingItem != null)
                {
                    //always remove the item existing before updating
                    RemoveItem(memoryCache, key);
                }
                return AddItem<T>(memoryCache, key, itemToCache);
            }
            return null;

        }

        /// <summary>
        /// Removes an item from the cache 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="memoryCache"></param>
        /// <returns></returns>
        public static bool RemoveItem(IMemoryCache memoryCache, Guid key)
        {
            lock (_locker)
            {
                if (memoryCache.TryGetValue(key, out var item))
                {
                    if (item != null)
                    {

                    }
                    memoryCache.Remove(key);
                    return true;
                }
            }
            return false;
        }

    }
}
