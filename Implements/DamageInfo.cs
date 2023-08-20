using System;
using Proto.PoolingSystem;
using UnityEngine;

[Serializable]
public class DamageInfo
{
    public Actor Attacker;
    public float Damage;
    public float GuardDamage;
    public Vector2 Direction;
    public bool GuardCrash;
    public bool Burn;
    public bool Stun;
    public bool Blind;
    public bool Slow;

    public DamageInfo Clone => new DamageInfo
    {
        Attacker = Attacker,
        Damage = Damage,
        GuardDamage = GuardDamage,
        Direction = Direction,
        GuardCrash = GuardCrash,
        Burn = Burn,
        Stun = Stun,
        Blind = Blind,
        Slow = Slow
    };
}