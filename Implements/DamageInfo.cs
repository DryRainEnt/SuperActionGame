using System;
using Proto.PoolingSystem;
using SimpleActionFramework.Core;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct DamageInfo
{
    public float Damage;
    public float GuardDamage;
    public Vector2 Direction;
    public float KnockbackPower;
    public float GuardCrash;

    public string NextSelfStateOnHit;
    public string NextOtherStateOnHit;
    public string NextSelfStateOnGuard;
    public string NextOtherStateOnGuard;
}