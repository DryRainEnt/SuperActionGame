using System;
using UnityEngine;

namespace SimpleActionFramework.Core
{
	public struct InputRecord : IComparable
	{
		public readonly string Key;

		public readonly float TimeStamp;

		public InputRecord(string key, float time)
		{
			Key = key;
			TimeStamp = time;
		}
		
		public int CompareTo(object obj)
		{
			if (obj is not InputRecord other)
				return 0;
            
			if (TimeStamp < other.TimeStamp)
				return 1;
			if (TimeStamp > other.TimeStamp)
				return -1;
			
			return 0;
		}

		public override string ToString()
		{
			return Key;
		}
	}
}