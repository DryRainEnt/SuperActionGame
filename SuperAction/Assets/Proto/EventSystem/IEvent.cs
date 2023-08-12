using System;
using UnityEngine.Events;

namespace Proto.EventSystem
{
    public abstract class IEvent : IDisposable
    {
        private static UnityEvent<IEvent> _listeners { get; set; } = new UnityEvent<IEvent>();

        public abstract void Dispose();
    }
}
