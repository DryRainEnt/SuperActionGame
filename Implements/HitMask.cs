using System;
using System.Collections.Generic;
using Proto.BasicExtensionUtils;
using Proto.PoolingSystem;
using SimpleActionFramework.Core;
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

    public class HitMask : IComparable
    {
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
        
        private List<HitMask> _hitThisFrame = new List<HitMask>();
        private List<HitMask> _hitRecord = new List<HitMask>();
        public HitMask firstHit { get; private set; }

        public DamageInfo Info;
    
        public Action OnHit;
        public Action OnMiss;
        
        private static TinyObjectPool<HitMask> pool
            = new TinyObjectPool<HitMask>();

        public static HitMask Create(MaskType type, Bounds bound, Actor owner, Action onHit = null, Action onMiss = null)
        {
            var e = pool.GetOrCreate();

            e.SetMask(type, bound, owner, onHit, onMiss);
            MaskManager.RegisterMask(e);
        
            return e;
        }

        public void Dispose()
        {
            MaskManager.UnregisterMask(this);
            ResetHitRecord();
            Owner = null;
            OnHit = null;
            OnMiss = null;
            StartTime = -1f;
            pool.Dispose(this);
        }
    
        private void SetMask(MaskType type, Bounds bound, Actor owner, Action onHit = null, Action onMiss = null)
        {
            Type = type;
            Owner = owner;
            _bounds = bound;
            OnHit = onHit;
            OnMiss = onMiss;
            
            StartTime = Time.realtimeSinceStartup;
        }

        // Update is called once per frame
        void Update()
        {
            _hitThisFrame.Clear();
        
            if (Info == null)
                return;
        }

        public void ResetHitRecord()
        {
            _hitRecord.Clear();
            _hitThisFrame.Clear();
            firstHit = null;
        }

        public void Record(HitMask mask)
        {
            _hitRecord.Add(mask);
            _hitThisFrame.Add(mask);
            firstHit ??= mask;
        }

        public bool CheckCollision(HitMask other)
        {
            if (other.Owner == Owner)
                return false;
            switch (other.Type | this.Type)
            {
                case MaskType.None:
                case MaskType.Static:
                case MaskType.Attack:
                case MaskType.Hit:
                case MaskType.Guard:
                case MaskType.Guard | MaskType.Hit:
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
}