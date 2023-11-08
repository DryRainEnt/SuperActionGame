using System.Collections;
using System.Collections.Generic;
using Proto.EventSystem;
using Resources.Scripts.Events;
using UnityEngine;

public class CameraTracker : MonoBehaviour, IEventListener
{
    public static CameraTracker Instance => _instance ? _instance : _instance = FindObjectOfType<CameraTracker>();
    private static CameraTracker _instance;

    private Transform T => transform;
    
    public void Track(Transform target, Vector2 offset)
    {
        T.SetParent(target);
        T.localPosition = offset;
    }
    
    public void Track(Vector2 position)
    {
        T.SetParent(null);
        T.position = position;
    }
    
    public void Untrack()
    {
        T.SetParent(null);
    }

    public bool OnEvent(IEvent e)
    {
        if (e is CallCameraTrackEvent callCameraTrackEvent)
        {
            Track(callCameraTrackEvent.target, Vector2.zero);
            return true;
        }

        return false;
    }
}
