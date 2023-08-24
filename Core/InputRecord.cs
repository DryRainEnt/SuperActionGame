using System;
using UnityEngine;

namespace SimpleActionFramework.Core
{
	public struct InputRecord : IComparable
	{
		public readonly string Key;

		public readonly float PressTime;

		private float _releaseTime;

		public float ReleaseTime
		{
			get => _releaseTime == 0 ? Time.realtimeSinceStartup : _releaseTime;
			set => _releaseTime = value;
		}
		
		public bool IsPressed => _releaseTime == 0;

		public InputRecord(string key, float press)
		{
			Key = key;
			PressTime = press;
			_releaseTime = 0f;
		}
		
		public int CompareTo(object obj)
		{
			if (obj is not InputRecord other)
				return 0;
            
			if (ReleaseTime < other.ReleaseTime)
				return 1;
			if (ReleaseTime > other.ReleaseTime)
				return -1;
            
			if (PressTime < other.PressTime)
				return 1;
			if (PressTime > other.PressTime)
				return -1;
			
			return 0;
		}

		public override string ToString()
		{
			return Key;
		}
	}
}