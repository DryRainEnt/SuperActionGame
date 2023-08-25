using System;
using System.Collections.Generic;
using Proto.EventSystem;
using Resources.Scripts.Events;
using SimpleActionFramework.Implements;
using UnityEngine;

namespace SimpleActionFramework.Core
{
	[Serializable]
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
		private static readonly List<HitData> HitDataList = new List<HitData>();
		
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
			HitDataList.Clear();
			
			for (var i = 0; i < HitMaskList.Count - 1; i++)
			{
				for (var j = i + 1; j < HitMaskList.Count; j++)
				{
					var mask = HitMaskList[i];
					var other = HitMaskList[j];
					if (!mask.CheckCollision(HitMaskList[j])) continue;
					
					HitDataList.Add(new HitData(){GiverMask = mask, ReceiverMask = other, DamageInfo = mask.Info});
				}
			}

			while (HitDataList.Count > 0)
			{
				var data = HitDataList[0];
				var other = data.ReceiverMask;
				var mask = data.GiverMask;

				IEvent e;

				switch (other.Type)
				{
					// Guard판정에 의해 Hit판정이 사라지는 경우
					// 서로 다른 두 충돌에 대해서
					// 1. 피격자가 서로 같고
					// 2. 한 쪽이 Guard, 나머지가 Hit일 때
					case MaskType.Guard 
				    when HitDataList.Exists(hit
						=> other.Owner == hit.ReceiverMask.Owner
						   && hit.ReceiverMask.Type == MaskType.Hit):
						HitDataList.RemoveAll(hit => other.Owner == hit.ReceiverMask.Owner && hit.ReceiverMask.Type == MaskType.Hit);
						data.DamageInfo = other.Info;
						e = OnAttackGuardEvent.Create(data);
						continue;
					// 중복된 Hit판정에 대해 하나만 남기는 경우
					// 서로 다른 두 충돌에 대해서
					// 1. 공격자가 서로 같고
					// 2. 피격자가 서로 같을 때
					case MaskType.Hit 
				    when HitDataList.Exists(hit
						=> mask.Owner == hit.GiverMask.Owner
						   && hit.GiverMask.Type == MaskType.Attack
						   && other.Owner == hit.ReceiverMask.Owner
						   && hit.ReceiverMask.Type == MaskType.Hit):
						HitDataList.RemoveAll(hit => mask.Owner == hit.GiverMask.Owner && other.Owner == hit.ReceiverMask.Owner && hit.ReceiverMask.Type == MaskType.Hit);
						HitDataList.Insert(0, data);
						e = OnAttackHitEvent.Create(data);
						break;
					default:
						HitDataList.RemoveAt(0);
						continue;
				}

				mask.Record(other);
				other.Record(mask);
				
				// 필요한 예외 사항은 전부 체크했으므로 이제 충돌 이벤트를 발생시키고 리스트에서 제거한다.
				// TODO: 이미 레코드 된 서로 같은 관계에 대해서는 이후에는 충돌 판정을 다시 하지 않는다.

				MessageSystem.Publish(e);
				
				HitDataList.RemoveAt(0);
			}
		}
	}
}