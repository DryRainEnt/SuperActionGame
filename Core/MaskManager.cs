using System.Collections.Generic;
using Proto.EventSystem;
using Resources.Scripts.Events;
using SimpleActionFramework.Implements;
using UnityEngine;

namespace SimpleActionFramework.Core
{
	public class HitData
	{
		public HitMask GiverMask;
		public HitMask ReceiverMask;
		public DamageInfo DamageInfo;
	}
	
	public static class MaskManager
	{
		/// <summary>
		/// HitMask는 아래의 순서로 정렬되어있다.
		/// Attack > Guard > Hit > Static
		/// </summary>
		private static readonly List<HitMask> HitMaskList = new List<HitMask>();
		
		public static void RegisterMask(HitMask mask)
		{
			HitMaskList.Add(mask);
			HitMaskList.Sort();
		}

		public static void UnregisterMask(HitMask mask)
		{
			HitMaskList.Remove(mask);
		}
		
		public static void Update()
		{
			var hitList = new List<(HitMask,HitMask)>();
			
			for (int i = 0; i < HitMaskList.Count - 1; i++)
			{
				for (int j = i + 1; j < HitMaskList.Count; j++)
				{
					var mask = HitMaskList[i];
					var other = HitMaskList[j];
					if (!mask.CheckCollision(HitMaskList[j])) continue;
					
					hitList.Add((mask, other));
				}
			}

			while (hitList.Count > 0)
			{
				var (mask, other) = hitList[0];
				
				// Guard판정에 의해 Hit판정이 사라지는 경우
				// 서로 다른 두 충돌에 대해서
				// 1. 피격자가 서로 같고
				// 2. 한 쪽이 Guard, 나머지가 Hit일 때
				if (other.Type == MaskType.Guard
				    && hitList.Exists(hit
					    => other.Owner == hit.Item2.Owner
					       && hit.Item2.Type == MaskType.Hit))
				{
					hitList.RemoveAll(hit => other.Owner == hit.Item2.Owner && hit.Item2.Type == MaskType.Hit);
					continue;
				}
				
				// 중복된 Hit판정에 대해 하나만 남기는 경우
				// 서로 다른 두 충돌에 대해서
				// 1. 공격자가 서로 같고
				// 2. 피격자가 서로 같을 때
				if (other.Type == MaskType.Hit
				    && hitList.Exists(hit
					    => mask.Owner == hit.Item1.Owner
					       && hit.Item1.Type == MaskType.Attack
					       && other.Owner == hit.Item2.Owner
					       && hit.Item2.Type == MaskType.Hit))
				{
					hitList.RemoveAll(hit => mask.Owner == hit.Item1.Owner && other.Owner == hit.Item2.Owner && hit.Item2.Type == MaskType.Hit);
					hitList.Insert(0, (mask, other));
				}
				
				mask.Record(other);
				other.Record(mask);
				
				// 필요한 예외 사항은 전부 체크했으므로 이제 충돌 이벤트를 발생시키고 리스트에서 제거한다.
				// TODO: 이미 레코드 된 서로 같은 관계에 대해서는 이후에는 충돌 판정을 다시 하지 않는다.

				var e = OnAttackHitEvent.Create(new HitData()
				{
					GiverMask = mask,
					ReceiverMask = other,
					DamageInfo = new DamageInfo()
				});
				MessageSystem.Publish(e);
				
				Debug.Log(e);
				
				hitList.RemoveAt(0);
			}
		}
	}
}