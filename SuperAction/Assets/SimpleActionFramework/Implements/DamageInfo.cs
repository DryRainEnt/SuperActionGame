using System;
using Proto.PoolingSystem;
using SimpleActionFramework.Core;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class DamageInfo
{
    public Actor Owner;
    public float Damage;
    public float GuardDamage;
    public Vector2 Direction;
    public bool GuardCrash;
    public DamageInfo Clone => new DamageInfo
    {
        Owner = Owner,
        Damage = Damage,
        GuardDamage = GuardDamage,
        Direction = Direction,
        GuardCrash = GuardCrash,
    };
}