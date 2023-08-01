namespace SimpleActionFramework.Core
{
    [System.Serializable]
    public class SingleActant
    {
        public ushort StartFrame;
        public ushort Duration;
        public ushort EndFrame => (ushort)(StartFrame + Duration);
    }
}
