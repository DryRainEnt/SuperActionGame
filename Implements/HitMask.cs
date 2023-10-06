using System;
using System.Collections.Generic;
using Proto.BasicExtensionUtils;
using Proto.PoolingSystem;
using SimpleActionFramework.Utility;
using UnityEngine;

namespace SimpleActionFramework.Core
{
    [Flags]
    public enum MaskType
    {
        None = 0x00,
        Attack = 0x01,
        Guard = 0x02,
        Hit = 0x04,
        Static = 0x08
    }

    public class HitMask : IComparable, IDisposable
    {
        public CombinedIdKey Id;
        
        public Actor Owner { get; private set; }
    
        public MaskType Type;
        private Bounds _bounds;

        public Bounds Bounds
        {
            get
            {
                if (Owner is null)
                    return _bounds;
            
                var center = Owner.IsLeft ? _bounds.center.FlipX() : _bounds.center;
                return new Bounds(center + Owner.transform.position, _bounds.size);
            }
        }

        public float StartTime { get; private set; }
        public float LifeTime { get; private set; }
        public bool IsAlive => Time.realtimeSinceStartup - StartTime < LifeTime;
        
        private List<HitMask> _hitThisFrame = new List<HitMask>();
        private List<HitMask> _hitRecord = new List<HitMask>();
        
        public HitMask firstHit { get; private set; }

        public DamageInfo Info;
        
        private static TinyObjectPool<HitMask> pool
            = new TinyObjectPool<HitMask>();

        public static HitMask Create()
        {
            var e = pool.GetOrCreate();
        
            return e;
        }

        public static HitMask Create(MaskType type, Bounds bound, Actor owner, DamageInfo info)
        {
            var e = pool.GetOrCreate();

            e.SetMask(type, bound, owner, info);
        
            return e;
        }

        public void Dispose()
        {
            MaskManager.UnregisterMask(this);
            ResetHitRecord();
            Owner = null;
            StartTime = -1f;
            pool.Dispose(this);
        }
    
        private void SetMask(MaskType type, Bounds bound, Actor owner, DamageInfo info)
        {
            Type = type;
            Owner = owner;
            _bounds = bound;
            Info = info;
            
            StartTime = Time.realtimeSinceStartup;
            
            MaskManager.RegisterMask(this);
        }

        // Update is called once per frame
        public void Update()
        {
            _hitThisFrame.Clear();
        }

        private void ResetHitRecord()
        {
            _hitRecord.Clear();
            _hitThisFrame.Clear();
            firstHit = null;
        }

        public bool Record(HitMask mask, DamageInfo info)
        {
            if (_hitRecord.Exists(hit => hit.Id.IsSameAction(mask.Id)))
                return false;
            if (_hitRecord.Exists(hit => hit.Owner == mask.Owner))
                return false;
            if (_hitThisFrame.Exists(hit => hit.Id.IsSameAction(mask.Id)))
                return false;
            
            _hitRecord.Add(mask);
            _hitThisFrame.Add(mask);
            firstHit ??= mask;

            var key = Constants.DefaultDataKeys[DefaultKeys.INTERACTION];
            switch ((Type, mask.Type))
            {
                case (MaskType.Attack, MaskType.Guard):
                    // 내 공격이 가드됨 => 가드 성공자의 가드 액션을 실행
                    Owner.ActionStateMachine.UpdateData(key, info.NextStateOnSuccessToReceiver);
                    break;
                case (MaskType.Attack, MaskType.Hit):
                    // 내 공격이 히트됨 => 내 히트 액션을 실행
                    Owner.ActionStateMachine.UpdateData(key, Info.NextStateOnSuccessToSelf);
                    break;
                case (MaskType.Attack, MaskType.Static):
                    // 내 공격이 무효화됨
                    break;
                case (MaskType.Guard, MaskType.Attack):
                    // 상대의 공격이 가드됨 => 내 가드 액션을 실행
                    Owner.ActionStateMachine.UpdateData(key, Info.NextStateOnSuccessToSelf);
                    break;
                case (MaskType.Hit, MaskType.Attack):
                    // 상대의 공격이 히트됨 => 공격 성공자의 히트 액션을 실행
                    Owner.ActionStateMachine.UpdateData(key, info.NextStateOnSuccessToReceiver);
                    break;
                case (MaskType.Hit, MaskType.Static):
                    break;
                default: break;
            }

            return true;
        }

        public bool CheckCollision(HitMask other)
        {
            if (other.Owner == Owner)
                return false;
            switch (other.Type | this.Type)
            {
                case MaskType.None:
                case MaskType.Attack:
                case MaskType.Guard:
                case MaskType.Hit:
                case MaskType.Static:
                case MaskType.Guard | MaskType.Hit:
                case MaskType.Guard | MaskType.Static:
                    return false;
                default: break;
            }
            return !_hitRecord.Exists(mask => mask.Owner == other.Owner) && Bounds.Intersects(other.Bounds);
        }

        public override string ToString()
        {
            return $"Owner : {Owner}\n" +
                   $"Type : {Type}\n" +
                   $"Bounds : {Bounds}\n";
        }

        public int CompareTo(object obj)
        {
            if (obj is not HitMask other)
                return 0;
            
            if (Type > other.Type)
                return 1;
            if (Type < other.Type)
                return -1;

            if (StartTime > other.StartTime)
                return 1;
            if (StartTime < other.StartTime)
                return -1;
            
            return 0;
        }
    }

    public static class MaskExtension
    {
        public static Color GetColor(this MaskType type) => type switch
        {
            MaskType.Attack => Color.red,
            MaskType.Guard => Color.cyan,
            MaskType.Hit => Color.green,
            MaskType.Static => Color.yellow,
            _ => Color.white
        };
    }
}