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

        public virtual void Act(float dt)
        {
            
        }
    }
}
