using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public delegate void Alarm();

public class Timer : IDisposable, IComparable<Timer>
{
    public float StartTime;
    public float? EndTime;

    public bool IsAutoDispose = false;

    public string Name = "";
    public Alarm Alarm;

    public Timer(string name = "")
    {
        if (name.Length > 0)
        {
            Name = name;
            TimerUtils.GlobalTimers.Add(name, this);
        }
        EndTime = null;
    }

    public Timer(float duration, string name = "")
    {
        StartTime = Time.realtimeSinceStartup;
        if (name.Length > 0)
        {
            EndTime = StartTime + duration;
            if (TimerUtils.GlobalTimers.ContainsKey(name))
            {
                TimerUtils.GlobalTimers[name].Dispose();
            }
            Name = name;
            TimerUtils.GlobalTimers.Add(name, this);
        }
        else
            SetAutoDispose(duration);
    }

    public void SetAutoDispose(float duration)
    {
        EndTime = StartTime + duration;
        IsAutoDispose = true;
        TimerUtils.Alarms.Add(this);
        TimerUtils.Alarms.Sort();
    }

    public void Reset()
    {
        StartTime = Time.realtimeSinceStartup;
        EndTime = null;

        IsAutoDispose = false;
        Alarm = null;
    }

    public void Stop()
    {
        EndTime = Time.realtimeSinceStartup;
        
        if (!IsAutoDispose) return;
        Alarm?.Invoke();
        Dispose();
    }

    public bool AlarmCheck()
    {
        if (EndTime > Time.realtimeSinceStartup) return false;
        Stop();
        return true;
    }

    public float GetDuration()
    {
        var et = EndTime ?? Time.realtimeSinceStartup;
        return et - StartTime;
    }

    public float GetElapse()
    {
        return Time.realtimeSinceStartup - StartTime;
    }

    public int CompareTo(Timer other)
    {
        if (other?.EndTime == null)
            return -1;
        if (this.EndTime < other.EndTime)
            return -1;
        return 1;
    }

    public override string ToString()
    {
        return ToString("");
    }

    public string ToString(string name)
    {
        if (name.Length > 1) name += "\n";
        return
            $"{name}StartTime : {StartTime} / EndTime : {EndTime} / Duration : {GetDuration() * 1000f}ms";
    }

    public void Dispose()
    {
        if (Name.Length > 0) TimerUtils.GlobalTimers.Remove(Name);
        TimerUtils.Alarms.Remove(this);
        IsAutoDispose = false;
        Alarm = null;
        Name = "";
    }
}

public static class TimerUtils
{
    /// <summary>
    /// 사용처에서 자체적으로 시간을 측정하는 경우
    /// </summary>
    public static Dictionary<string, Timer> GlobalTimers = new Dictionary<string, Timer>();
    
    /// <summary>
    /// 시스템에서 자동으로 시간을 체크해서 알려줘야 하는 경우
    /// </summary>
    public static List<Timer> Alarms = new List<Timer>();
}
