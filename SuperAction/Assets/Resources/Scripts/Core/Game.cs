using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Proto.EventSystem;
using Resources.Scripts.Core;
using Resources.Scripts.Events;
using SimpleActionFramework.Core;
using UnityEngine;

public class Game : MonoBehaviour, IEventListener
{
    private static Game _instance;
    public static Game Instance => _instance ? _instance : FindObjectOfType<Game>();
    
    public Actor Player;
    public Actor Learner;
    
    public Dictionary<int, Actor> RegisteredActors = new Dictionary<int, Actor>();
    
    [SerializeField]
    private UnityEngine.UI.Text _debugText;

    [SerializeField]
    private TMPro.TMP_Text _timerText;
    [SerializeField]
    private TMPro.TMP_Text _fpsText;
    
    private FrameDataChunk _frameDataChunk = new FrameDataChunk(1);

    public Vector2Int ScreenResolution => new Vector2Int(1280, 720) * ScreenResolutionFactor;
    public int ScreenResolutionFactor = 1;

    private bool _isWriting = false;
    public static bool IsWriting => Instance._isWriting;
    
    private bool _isPlayable = false;
    public static bool IsPlayable => Instance._isPlayable;

    // Start is called before the first frame update
    async void Start()
    {
        Application.targetFrameRate = 60;
        
        MessageSystem.Subscribe(typeof(OnDeathEvent), this);

        await Task.Run(NetworkManager.Instance.RunPython);
    }

    private void OnDisable()
    {
        MessageSystem.Unsubscribe(typeof(OnDeathEvent), this);
    }

    // Update is called once per frame
    void Update()
    {
        while (TimerUtils.Alarms.Count > 0)
            if (!TimerUtils.Alarms[0].AlarmCheck())
                break;
        
        if (GlobalInputController.Instance.GetInput("reset"))
        {
            foreach (var actor in RegisteredActors.Values)
            {
                actor.ResetPosition();
            }
        }

        if (GlobalInputController.Instance.GetPressed("resize"))
        {
            ScreenResolutionFactor++;
            if (ScreenResolutionFactor > 3)
                ScreenResolutionFactor = 1;
            Debug.Log(ScreenResolution);
            Screen.SetResolution(ScreenResolution.x, ScreenResolution.y, false);
        }
        
        _fpsText.text = $"{Mathf.Round((Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f) * 100f) * 0.01f : 00.00} fps";
        _timerText.text = $"{Time.realtimeSinceStartup:##00.00}";
    }

    private void FixedUpdate()
    {
        MaskManager.Instance.Update();

        _isPlayable = false;
        
        if (RegisteredActors.Count < 2 || !NetworkManager.Instance.isActive) return;
        
        _isPlayable = true;
        _frameDataChunk.Append(new FrameData(Time.frameCount));
    }

    public bool OnEvent(IEvent e)
    {
        if (e is OnDeathEvent { ActorIndex: 1 })
        {
            WriteFrameDataExternal();
            return true;
        }
        
        if (e is OnAttackHitEvent ahe)
        {
            var healthDiff = ahe.info.Damage * (ahe.takerMask.Owner == Learner ? -1 : 1);
            var currentFrame = Time.frameCount;
            foreach (var frame in _frameDataChunk.Frames)
            {
                var elapsed = currentFrame - frame.Frame;
                var valid = frame.Aggressiveness * healthDiff / (30 + elapsed);
                frame.AddValidation(valid);
            }
            return true;
        }
        
        return false;
    }

    public Actor GetEnemy(int self)
    {
        return RegisteredActors[self == 0 ? 1 : 0];
    }

    public async void WriteFrameDataExternal()
    {
        _isWriting = true;
        var task = Task.Run(WriteFrameData);
        
        // 함수를 리턴하고 태스크가 종료될 때까지 기다린다.
        // 따라서 바로 "Run() returns" 로그가 출력된다.
        // 태스크가 끝나면 result 에는 CountAsync() 함수의 리턴값이 저장된다.
        int result = await task;

        // 태스크가 끝나면 await 바로 다음 줄로 돌아와서 나머지가 실행되고 함수가 종료된다.
        //Debug.Log("Result : " + result);

        await NetworkManager.Instance.StartLearning();
        
        _isWriting = false;
    }

    private async Task<int> WriteFrameData()
    {
        int result = 0;

        Debug.Log("Writing...");
        string dir = Application.streamingAssetsPath + $"/FrameData/{DateTime.Now:yyMMdd-hhmm}";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        FileStream saveStream
            = new FileStream(dir + $"/frameData_{NetworkManager.Instance.NeuralNetwork.level}.json", 
                FileMode.OpenOrCreate, FileAccess.Write);
        
        await using StreamWriter saveWriter = new StreamWriter(saveStream);
        await saveWriter.WriteAsync(_frameDataChunk.ToJson());
        saveWriter.Close();
        Debug.Log("Backup Done");
        
        FileStream fileStream
            = new FileStream(Application.streamingAssetsPath + "/data.json", 
                FileMode.Create, FileAccess.Write);
        
        await using StreamWriter writer = new StreamWriter(fileStream);
        await writer.WriteAsync(_frameDataChunk.ToJson());
        _frameDataChunk.Clear();
        writer.Close();
        Debug.Log("Writing Complete!");
        
        result = 1;

        return result;
    }

    private void OnDrawGizmos()
    {
        _debugText.text = $"Masks: {MaskManager.HitMaskList.Count}";
        
        foreach (var mask in MaskManager.HitMaskList)
        {
            var c = mask.Type.GetColor();
            c.a = 0.5f;
            Gizmos.color = c;
            Gizmos.DrawCube(mask.Bounds.center, mask.Bounds.size);
        }

        foreach (var hit in MaskManager.HitDataList)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere((hit.GiverMask.Bounds.center + hit.ReceiverMask.Bounds.center) * 0.5f, 0.3f);
        }
    }
}
