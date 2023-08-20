using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleActionFramework.Core
{
	public static class BoundMaskHandler
	{
		public static Dictionary<UnityEngine.Object, BoundMask> MaskMap = new Dictionary<UnityEngine.Object, BoundMask>();
		
		public static bool RegisterMask(UnityEngine.Object self, BoundMask maskData)
		{
			if (self is null || MaskMap.ContainsKey(self))
				return false;
			
			MaskMap.Add(self, maskData);

			return true;
		}
		
		public static bool UnregisterMask(UnityEngine.Object self)
		{
			if (self is null || !MaskMap.ContainsKey(self))
				return false;
			
			MaskMap.Remove(self);

			return true;
		}

		public static void Validate()
		{
			foreach (var (obj, data) in MaskMap)
			{
			}
		}
        
		public static bool IsCollided(BoundMask self, BoundMask other)
		{
			if (self.Collider == other.Collider)
				return false;
			if (self.Owner == other.Owner)
				return false;
			if (self.Type == other.Type)
				return false;
			
			
			var selfBounds = self.Collider.bounds;
			var otherBounds = other.Collider.bounds;

			return selfBounds.Intersects(otherBounds);
		}
	}

	public enum MaskType
	{
		Attack,
		Guard,
		Hit,
		Static,
	}
	
	public class BoundMask
	{
		public MaskType Type;
		public Actor Owner;
		public Collider2D Collider;

		public Action<BoundMask> OnAttackHit;
		public Action<BoundMask> OnGuard;
		public Action<BoundMask> OnDamaged;
	}
}