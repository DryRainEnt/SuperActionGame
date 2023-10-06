using System;
using SimpleActionFramework.Utility;
using UnityEngine;

namespace SimpleActionFramework.Core
{
    public enum ActantState
    {
        NotStarted,
        Running,
        Finished
    }
    
    [Serializable]
    public class SingleActant : IDisposable
    {
        public CombinedIdKey id;
        
        public int StartFrame;
        public int Duration;
        public bool UsedOnce => Duration == 0;
        public int EndFrame => StartFrame + Duration;
        public int DrawnFrame => Mathf.Max(StartFrame + 1, StartFrame + Duration);

        public InterpolationType InterpolationType;
        protected float InnerProgress;
        protected float PrevProgress;
        
        public int Idx;
        
        public CombinedIdKey GetId(Actor actor)
        {
            return new CombinedIdKey(actor.ActionStateMachine.GetId, actor.ActionStateMachine.CurrentState.GetId, Idx);
        }

        public virtual void Act(Actor actor, float progress, bool isFirstFrame = false)
        {
            PrevProgress = InnerProgress;
            InnerProgress = InterpolationType.Interpolate(progress);
        }
        
        public virtual void OnFinished(Actor actor) { }
        
        public virtual void OnStart(Actor actor) { }
        
        public virtual void OnReset(Actor actor) { }
        
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
