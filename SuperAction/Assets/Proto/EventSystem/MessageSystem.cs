using System;
using System.Collections;
using System.Collections.Generic;
using Proto.PoolingSystem;
using SimpleActionFramework.Core;
using UnityEngine;

namespace Proto.EventSystem
{
    [SerializeField]
    public delegate bool EventSubscription(IEvent e);

    public class EventSubscriptionInfo : IEquatable<EventSubscriptionInfo>
    {
        public EventSubscription callback;
        public readonly IEventListener listener;

        public EventSubscriptionInfo(IEventListener listener)
        {
            this.listener = listener;
            this.callback = listener.OnEvent;
        }

        public bool Equals(EventSubscriptionInfo other)
        {
            return Equals(listener, other?.listener);
        }

        public override bool Equals(object obj)
        {
            return obj?.GetType() == this.GetType() && Equals((EventSubscriptionInfo) obj);
        }

        public override int GetHashCode()
        {
            return listener.GetHashCode();
        }
    }

    [Serializable]
    public class EventSubscriptionBook : SerializedDictionary<string, string>{}
    public class MessageSystem : MonoBehaviour
    {
        public static MessageSystem Instance;

        [SerializeField]
        public EventSubscriptionBook EventSubscriptions = new EventSubscriptionBook();

        private static Dictionary<Type, List<EventSubscriptionInfo>> _eventSubscriptions
            = new Dictionary<Type, List<EventSubscriptionInfo>>();

        private static List<KeyValuePair<Type, IEventListener>> _eventSubscriptionCalls 
            = new List<KeyValuePair<Type, IEventListener>>();
    
        private static List<KeyValuePair<Type, IEventListener>> _eventUnsubscriptionCalls 
            = new List<KeyValuePair<Type, IEventListener>>();

        private static List<PublishedEvent> _publishedEvents = new List<PublishedEvent>();

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            foreach (var pair in _eventSubscriptions)
            {
                var type = pair.Key;
                var subs = pair.Value;
                if (!EventSubscriptions.ContainsKey(type.ToString())) continue;
                foreach (var sub in subs)
                {
                    EventSubscriptions[type.ToString()] = 
                        Utils.BuildString(sub.listener.ToString().Split('(')[0], " | ");
                }
                var str = EventSubscriptions[type.ToString()];
                EventSubscriptions[type.ToString()] = str.Trim(' ', '|');
            }
        }

        private void LateUpdate()
        {
            //신규 구독 요청 처리
            foreach (var pair in _eventSubscriptionCalls)
            {
                var type = pair.Key;
                var listener = pair.Value;
                if (!_eventSubscriptions.ContainsKey(type))
                {
                    _eventSubscriptions.Add(type, new List<EventSubscriptionInfo>());
                    EventSubscriptions.Add(type.ToString(), "");
                }
                if (!_eventSubscriptions[type].Exists(x => x.listener == listener))
                    _eventSubscriptions[type].Add(new EventSubscriptionInfo(listener));
            }
        
            _eventSubscriptionCalls.Clear();
        
            //구독 해지 요청 처리
            foreach (var pair in _eventUnsubscriptionCalls)
            {
                var type = pair.Key;
                var listener = pair.Value;
                if (!_eventSubscriptions.ContainsKey(type))
                {
                    continue;
                }
                _eventSubscriptions[type].RemoveAll(l => l.listener == listener);

                if (_eventSubscriptions[type].Count == 0)
                {
                    _eventSubscriptions.Remove(type);
                    EventSubscriptions.Remove(type.ToString());
                }
            }

            _eventUnsubscriptionCalls.Clear();


            PublishedEvent[] _publishedThisFrame = new PublishedEvent[_publishedEvents.Count];
            _publishedEvents.CopyTo(_publishedThisFrame);
            //구독한 청취자들에게 발행된 이벤트 전달
            foreach (var pe in _publishedThisFrame)
            {
                //대상이 특정되면 이벤트 전달
                if (pe.target != null)
                {
                    pe.target.OnEvent(pe.e);
                    continue;
                }
                
                var eventType = pe.e.GetType();
                if (!_eventSubscriptions.ContainsKey(eventType)) continue;
                //전체 발행용 이벤트
                if (pe.target == null)
                {
                    foreach (var target in _eventSubscriptions[eventType])
                    {
                        target.callback.Invoke(pe.e);
                    }

                }
                //발행된 이벤트들이 다시 이벤트를 발행하는 경우가 있어, 목록이 변조되고있다.
                //이벤트 내에서 이벤트 발행을 지양해야 하지만, 안전을 위해 옮겨두고 처리한다.
                _publishedEvents.Remove(pe);
            }
        
        
        }

        public static void Subscribe(Type e, IEventListener listener)
        {
            _eventSubscriptionCalls.Add(
                new KeyValuePair<Type, IEventListener>(e, listener));
        }
    
        public static void Unsubscribe(Type e, IEventListener listener)
        {
            _eventUnsubscriptionCalls.Add(
                new KeyValuePair<Type, IEventListener>(e, listener));
        }

        /// <summary>
        /// 구독중인 모든 대상에게 이벤트를 발행한다.
        /// </summary>
        /// <param name="e"></param>
        public static void Publish(IEvent e)
        {
            _publishedEvents.Add(new PublishedEvent(){target = null, e = e});
        }
    
        /// <summary>
        /// 구독중인 특정 대상에게 이벤트를 전달한다.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void Send(IEventListener target, IEvent e)
        {
            _publishedEvents.Add(new PublishedEvent(){target = target, e = e});
        }
    }

    public struct PublishedEvent
    {
        public IEventListener target;

        public IEvent e;
    }
}