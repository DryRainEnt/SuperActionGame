using System;
using System.Collections.Generic;
using Proto.PoolingSystem;

namespace SimpleActionFramework.Utility
{
	public class PooledList<T> : List<T>, IDisposable
	{
		private static TinyObjectPool<PooledList<T>> pool
			= new TinyObjectPool<PooledList<T>>();

		public static PooledList<T> Create()
		{
			var e = pool.GetOrCreate();
			return e;
		}

		public void Dispose()
		{
			Clear();
			pool.Dispose(this);
		}
	}
}