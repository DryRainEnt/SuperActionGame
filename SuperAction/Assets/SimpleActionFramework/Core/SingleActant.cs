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
    public class SingleActant
    {
        private ActantState _state = ActantState.NotStarted;

        public ActantState State
        {
            get => _state;
            set
            {
                if (_state == value)
                    return;
                
                _state = value;
                if (_state == ActantState.NotStarted)
                    OnReset();
                if (_state == ActantState.Running)
                    OnStart();
                if (_state == ActantState.Finished)
                    OnFinished();
            }
        }
        
        public int StartFrame;
        public int Duration;
        public bool UsedOnce => Duration == 0;
        public int EndFrame => StartFrame + Duration;
        public int DrawnFrame => Mathf.Max(StartFrame + 1, StartFrame + Duration);
        
        protected Actor Actor;
        protected ActionStateMachine Machine => Actor.ActionStateMachine;

        public InterpolationType InterpolationType;
        protected float InnerProgress;
        protected float PrevProgress;
        
        public void Init(Actor actor)
        {
            State = ActantState.NotStarted;
            Actor = actor;
            OnReset();
        }

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
            return this.GetType().ToString().Split('.')[^1].Replace("Actant", "");
        }
    }
}
