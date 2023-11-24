using System;
using System.Collections.Generic;
using Proto.EventSystem;
using Resources.Scripts.Events;
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
	
	public class MaskManager : MonoBehaviour
	{
		private static MaskManager _instance;
		public static MaskManager Instance => _instance ? _instance : _instance = FindObjectOfType<MaskManager>();
		
		/// <summary>
		/// HitMask는 아래의 순서로 정렬되어있다.
		/// Attack > Guard > Hit > Static
		/// </summary>
		public static readonly List<HitMask> HitMaskList = new List<HitMask>();
		public static readonly List<HitData> HitDataList = new List<HitData>();
		
		public static void RegisterMask(HitMask mask)
		{
			HitMaskList.Add(mask);
			HitMaskList.Sort();
		}

		public static void UnregisterMask(HitMask mask)
		{
			HitMaskList.Remove(mask);
		}

		private void OnApplicationQuit()
		{
			while (HitMaskList.Count > 0)
			{
				var mask = HitMaskList[0];
				HitMaskList.RemoveAt(0);
				mask.Dispose();
			}
            HitMaskList.Clear();
			HitDataList.Clear();
		}

		public void Update()
		{
			HitDataList.Clear();
			
			for (var i = 0; i < HitMaskList.Count; i++)
			{
                var mask = HitMaskList[i];
                mask.Update();
                
				for (var j = i + 1; j < HitMaskList.Count; j++)
				{
					var other = HitMaskList[j];
					if (!mask.CheckCollision(HitMaskList[j])) continue;
					
					HitDataList.Add(new HitData(){GiverMask = mask, ReceiverMask = other, DamageInfo = mask.Info});
				}
			}

			while (HitDataList.Count > 0)
			{
				var data = HitDataList[0];
				var mask = data.GiverMask;
				var other = data.ReceiverMask;

				IEvent e;

				switch (other.Type)
				{
					// Guard판정에 의해 Hit판정이 사라지는 경우
					// 서로 다른 두 충돌에 대해서
					// 1. 공격자와 피격자가 서로 같고
					// 2. 한 쪽이 Guard, 나머지가 Hit일 때
					// => Hit판정을 제거하고 Guard판정을 남긴다.
					case MaskType.Guard :
						HitDataList.RemoveAll(hit => mask.Owner == hit.GiverMask.Owner
						                             && other.Owner == hit.ReceiverMask.Owner
						                             && hit.ReceiverMask.Type == MaskType.Hit);
						data.DamageInfo = other.Info;
						data.DamageInfo.Point = (mask.Bounds.center + other.Bounds.center) / 2f;
						data.DamageInfo.Color = other.Owner.Color;
						data.DamageInfo.GuardDamage = mask.Info.Damage;
						e = OnAttackGuardEvent.Create(data);
						break;
					// 중복된 Hit판정에 대해 하나만 남기는 경우
					// 서로 다른 두 충돌에 대해서
					// 1. 공격자와 피격자가 서로 같고
					// 2. 여러 Hit가 동시에 존재할 때
					// => Hit판정을 제거하고 가장 먼저 발생한 Hit판정을 남긴다.
					case MaskType.Hit :
						HitDataList.RemoveAll(hit => mask.Owner == hit.GiverMask.Owner && other.Owner == hit.ReceiverMask.Owner && hit.GiverMask.Type == MaskType.Attack && hit.ReceiverMask.Type == MaskType.Hit);
						HitDataList.Insert(0, data);
						data.DamageInfo = mask.Info;
						data.DamageInfo.Point = (mask.Bounds.center + other.Bounds.center) / 2f;
						data.DamageInfo.Color = mask.Owner.Color;
						e = OnAttackHitEvent.Create(data);
						break;
					default:
						HitDataList.RemoveAt(0);
						continue;
				}

				mask.Record(other, data.DamageInfo);
				other.Record(mask, data.DamageInfo);
				
				MessageSystem.Publish(e);
				
				HitDataList.RemoveAt(0);
			}
		}
	}
}