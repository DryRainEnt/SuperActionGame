using System.Collections;
using System.Collections.Generic;
using SimpleActionFramework.Core;
using UnityEngine;

public class Character : MonoBehaviour
{
    public ActionStateMachine ActionStateMachine;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var dt = Time.deltaTime;
        
        ActionStateMachine.UpdateState(dt);
    }
}
