using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance => _instance ? _instance : _instance = FindObjectOfType<GameUIManager>();
    private static GameUIManager _instance;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class GenericUICallData
{
    public Vector2 Position;
    public Vector2 Size;
}

public class GenericUIReturnData
{
    
}
