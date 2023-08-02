using UnityEngine;

namespace SimpleActionFramework.Core
{
    [System.Serializable]
    public class SingleActant
    {
        public ushort StartFrame;
        public ushort Duration;
        public bool UsedOnce => Duration == 0;
        public ushort EndFrame => (ushort)(StartFrame + Duration);

        protected float InnerProgress;

        public virtual void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
        {
            InnerProgress = progress;
        }
    }
}
