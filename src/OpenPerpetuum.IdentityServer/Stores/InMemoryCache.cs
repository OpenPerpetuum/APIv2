using IdentityServer4.Services;
using OpenPerpetuum.Core.Extensions.Threading;
using OpenPerpetuum.Core.Foundation.Processing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenPerpetuum.IdentityServer.Stores
{
	public class InMemoryCache<TObject> : ICache<TObject>
		where TObject : class
	{
		private class CacheItem
		{
			public TObject DataItem
			{
				get;
				set;
			}

			public DateTimeOffset AbsoluteExpiry
			{
				get;
				set;
			}
		}
		private readonly TimeSpan THREAD_TIMEOUT = TimeSpan.FromSeconds(1);

		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		private readonly Dictionary<string, CacheItem> objects = new Dictionary<string, CacheItem>();
		private readonly IGenericContext genericContext;

		public InMemoryCache(IGenericContext genericContext)
		{
			this.genericContext = genericContext;
		}

		public Task<TObject> GetAsync(string key)
		{
			return Task.Run(() =>
			{
				CacheItem item;

				using (_lock.Read(THREAD_TIMEOUT))
				{
					if (!objects.TryGetValue(key, out item))
						return null;
				}

				if (genericContext.CurrentDateTime > item.AbsoluteExpiry)
				{
					using (_lock.Write(THREAD_TIMEOUT))
						if (objects.ContainsKey(key)) objects.Remove(key);

					return null;
				}

				return item.DataItem;
			});
		}

		public Task SetAsync(string key, TObject item, TimeSpan expiration)
		{
			return Task.Run(() =>
			{
				using (_lock.Write(THREAD_TIMEOUT))
				{
					CacheItem cacheItem = new CacheItem
					{
						DataItem = item,
						AbsoluteExpiry = genericContext.CurrentDateTime.Add(expiration)
					};

					if (objects.ContainsKey(key))
						objects[key] = cacheItem;
					else
						objects.Add(key, cacheItem);
				}
			});
		}
	}
}
