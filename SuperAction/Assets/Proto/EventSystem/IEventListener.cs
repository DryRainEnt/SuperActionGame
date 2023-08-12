namespace Proto.EventSystem
{
    public interface IEventListener
    {
        bool OnEvent(IEvent e);
    }
}
