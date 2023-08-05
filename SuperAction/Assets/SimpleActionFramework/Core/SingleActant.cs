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
        
        public ushort StartFrame;
        public ushort Duration;
        public bool UsedOnce => Duration == 0;
        public ushort EndFrame => (ushort)(StartFrame + Duration);
        
        protected ActionStateMachine Machine;

        protected float InnerProgress;
        
        public void Init(ActionStateMachine machine)
        {
            State = ActantState.NotStarted;
            Machine = machine;
            OnReset();
        }

        public virtual void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
        {
            InnerProgress = progress;
        }
        
        public virtual void OnFinished() { }
        
        public virtual void OnStart() { }
        
        public virtual void OnReset() { }
    }
}
