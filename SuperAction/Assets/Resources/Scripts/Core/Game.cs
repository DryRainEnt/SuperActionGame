using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Proto.EventSystem;
using Proto.PoolingSystem;
using Resources.Scripts.Core;
using Resources.Scripts.Events;
using SimpleActionFramework.Core;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour, IEventListener
{
    private static Game _instance;
    public static Game Instance => _instance ? _instance : FindObjectOfType<Game>();
    
    public Dictionary<int, Actor> RegisteredActors = new Dictionary<int, Actor>();
    
    [SerializeField]
    private UnityEngine.UI.Text _debugText;

    [SerializeField]
    private TMPro.TMP_Text _timerText;
    [SerializeField]
    private TMPro.TMP_Text _fpsText;
    
    [SerializeField]
    private UnityEngine.UI.Slider _cpuCountSlider;
    
    private FrameDataChunk _frameDataChunk = new FrameDataChunk(1);

    public Vector2Int ScreenResolution => new Vector2Int(1280, 720) * ScreenResolutionFactor;
    public int ScreenResolutionFactor = 1;

    private bool _isWriting = false;
    public static bool IsWriting => Instance._isWriting;
    
    private bool _isPlayable = false;
    public static bool IsPlayable => Instance._isPlayable;

    private float _startTime = 0f;
    
    public List<int> survivedPlayers = new List<int>();
    
    public TMP_Text LifeCountText;
    public TMP_Text KillCountText;

    public int playerLifeCount
    {
        set => LifeCountText.text = $"{value}";
    }
    public int playerKillCount
    {
        set => KillCountText.text = $"{value}";
    }

    private void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    async void Start()
    {
        ObjectPoolController.GetOrCreate("Actor", "Characters");
        Application.targetFrameRate = 60;

        // Out game scene
    }

    public int RegisterActor(Actor actor)
    {
        var id = RegisteredActors.Count;
        RegisteredActors.Add(id, actor);
        actor.ActorIndex = id;

        return id;
    }

    public void UnregisterActor(Actor actor)
    {
        RegisteredActors.Remove(actor.ActorIndex);
    }

    public void UnregisterActor(int id)
    {
        RegisteredActors.Remove(id);
    }
    
    public async Task StartGame(StageData stage)
    {
        MessageSystem.Subscribe(typeof(OnDeathEvent), this);
        MessageSystem.Subscribe(typeof(OnReviveEvent), this);
        MessageSystem.Subscribe(typeof(OnGameEndEvent), this);

        CameraTracker.Instance.Track(Vector2.up * 24f);

        // Test use
        var participants = stage.Participants;

        foreach (var data in participants)
        {
            var player = ObjectPoolController.InstantiateObject("Actor",
                new PoolParameters(new Vector2(6 * UnityEngine.Random.Range(-3, 4), 12))) as Actor;
            player.ChangeActorControl(data.ControllerType);
            player.OriginColor = data.Color;
            player.Initiate();
            player.networkObject.Spawn();
            RegisterActor(player);
            player.gameObject.SetActive(false);

            survivedPlayers.Add(player.ActorIndex);
        }

        AudienceController.Instance.SummonAudience();

        await Task.Delay(3000);

        _isPlayable = true;
        _startTime = Time.realtimeSinceStartup;

        foreach (var pair in RegisteredActors)
        {
            var actor = pair.Value;
            StartCoroutine(actor.OnRevive());
        }
        
        await Task.Run(NN_Manager.Instance.RunPython);
    }

    public async Task JoinGame()
    {
        //TODO: Join game
    }

    public async Task EndGame()
    {
        MessageSystem.Unsubscribe(typeof(OnDeathEvent), this);
        MessageSystem.Unsubscribe(typeof(OnReviveEvent), this);
        MessageSystem.Unsubscribe(typeof(OnGameEndEvent), this);
    }

    private async void OnDisable()
    {
        await EndGame();
    }

    // Update is called once per frame
    void Update()
    {
        while (TimerUtils.Alarms.Count > 0)
            if (!TimerUtils.Alarms[0].AlarmCheck())
                break;

        if (GlobalInputController.Instance.GetPressed("resize"))
        {
            ScreenResolutionFactor++;
            if (ScreenResolutionFactor > 3)
                ScreenResolutionFactor = 1;
            Debug.Log(ScreenResolution);
            Screen.SetResolution(ScreenResolution.x, ScreenResolution.y, false);
        }
        
        _fpsText.text = $"{Mathf.Round((Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f) * 100f) * 0.01f : 00.00} fps";

        if (_isPlayable)
            _timerText.text = $"{Time.realtimeSinceStartup - _startTime:##00.00}";
    }

    private void FixedUpdate()
    {
        MaskManager.Instance.Update();
        
        if (RegisteredActors.Count < 2 || !NN_Manager.Instance.isActive) return;
        
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
            var healthDiff = ahe.info.Damage 
                             * (ahe.takerMask.Owner.controllerType == 3 ? -1 : 1);
            var currentFrame = Time.frameCount;
            foreach (var frame in _frameDataChunk.Frames)
            {
                var elapsed = currentFrame - frame.Frame;
                var valid = frame.Aggressiveness * healthDiff / (30 + elapsed);
                frame.AddValidation(valid);
            }
            return true;
        }
        
        if (e is OnGameEndEvent oge)
        {
            _isPlayable = false;
            Debug.Log($"Winner is : {oge.WinnerIndex}");
            
            foreach (var actor in RegisteredActors.Values)
            {
                actor.gameObject.SetActive(false);
                
            }
            
            return true;
        }
        
        return false;
    }

    public Actor GetEnemy(int self)
    {
        return RegisteredActors[self == 0 ? 1 : 0];
    }

    public Actor GetClosestEnemy(int self)
    {
        return RegisteredActors.Where(i => i.Key != self)
            .OrderBy(j => Vector2.Distance(
                RegisteredActors[self].Position, j.Value.Position))
            .First().Value;
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

        await NN_Manager.Instance.StartLearning();
        
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
            = new FileStream(dir + $"/frameData_{NN_Manager.Instance.NeuralNetwork.level}.json", 
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
