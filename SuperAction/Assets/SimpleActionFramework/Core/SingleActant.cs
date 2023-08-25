using System;
using UnityEngine;

namespace SimpleActionFramework.Core
{
    public enum ActantState
    {
        NotStarted,
        Running,
        Finished
    }
    
    [System.Serializable]
    public class SingleActant : IDisposable
    {
        public int StartFrame;
        public int Duration;
        public bool UsedOnce => Duration == 0;
        public int EndFrame => StartFrame + Duration;
        public int DrawnFrame => Mathf.Max(StartFrame + 1, StartFrame + Duration);

        public InterpolationType InterpolationType;
        protected float InnerProgress;
        protected float PrevProgress;

        public virtual void Act(Actor actor, float progress, bool isFirstFrame = false)
        {
            PrevProgress = InnerProgress;
            InnerProgress = InterpolationType.Interpolate(progress);
        }
        
        public virtual void OnFinished() { }
        
        public virtual void OnStart() { }
        
        public virtual void OnReset() { }
        
        public virtual void OnGUI(Rect position, float scale, float progress) { }
        
        public virtual void OnDrawGizmos() { }

        public override string ToString()
        {
            return this.GetType().ToString().Split('.')[^1].Replace("Set", "").Replace("Actant", "");
        }
        
        public virtual void CopyFrom(SingleActant actant)
        {
            StartFrame = actant.StartFrame;
            Duration = actant.Duration;
            InterpolationType = actant.InterpolationType;
        }

        public virtual void Dispose()
        {
            // TODO release managed resources here
        }
    }
}
