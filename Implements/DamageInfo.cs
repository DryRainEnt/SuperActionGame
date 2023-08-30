﻿using System;
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

    /// <summary>
    /// 상대를 향한 의도가 적중했을 때, 자신에게 기록할 다음 상태
    /// </summary>
    public string NextStateOnSuccessToSelf;
    /// <summary>
    /// 상대를 향한 의도가 적중했을 때, 상대에게 기록할 다음 상태
    /// </summary>
    public string NextStateOnSuccessToReceiver;
}