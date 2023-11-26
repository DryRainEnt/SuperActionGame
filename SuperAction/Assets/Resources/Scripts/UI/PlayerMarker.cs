using System.Collections;
using System.Collections.Generic;
using Proto.EventSystem;
using Proto.PoolingSystem;
using Resources.Scripts.Events;
using SimpleActionFramework.Core;
using UnityEngine;

public class PlayerMarker : MonoBehaviour, IPooledObject, IEventListener
{
    public Actor Target;
    public int ActorId => Target.ActorIndex;
    public Vector3 Offset;

    public string Name { get; set; } = "PlayerMarker";

    public void Dispose()
    {
        Target = null;
        MessageSystem.Unsubscribe(typeof(OnDeathEvent), this);
        ObjectPoolController.Dispose(this);
    }

    public void OnPooled()
    {
        Offset = Vector2.up * 3f;
        Target = Game.Player;
        MessageSystem.Subscribe(typeof(OnDeathEvent), this);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Target)
        {
            transform.position = Target.transform.position + Offset;
        }
    }

    public bool OnEvent(IEvent e)
    {
        if (e is OnDeathEvent ode)
        {
            if (ode.ActorIndex == ActorId)
            {
                Dispose();
                return true;
            }
        }
        return false;
    }
}
