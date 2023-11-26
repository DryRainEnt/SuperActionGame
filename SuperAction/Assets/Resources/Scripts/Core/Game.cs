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
    
    [SerializeField]
    private FrameDataChunk _frameDataChunk = new FrameDataChunk(1);
    public FrameDataChunk FrameDataChunk => _frameDataChunk;

    public Vector2Int ScreenResolution => new Vector2Int(1280, 720) * ScreenResolutionFactor;
    public int ScreenResolutionFactor = 1;

    private bool _isWriting = false;
    public static bool IsWriting => Instance._isWriting;
    
    private bool _isPlayable = false;
    public static bool IsPlayable => Instance._isPlayable;

    private float _startTime = 0f;

    private void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    async void Start()
    {
        Application.targetFrameRate = 60;
        
        MessageSystem.Subscribe(typeof(OnDeathEvent), this);
        MessageSystem.Subscribe(typeof(OnReviveEvent), this);
        MessageSystem.Subscribe(typeof(OnAttackHitEvent), this);
        MessageSystem.Subscribe(typeof(OnAttackGuardEvent), this);
        
        RegisteredActors.Add(0, Player);
        RegisteredActors.Add(1, Learner);
        
        Player.Initiate();
        Learner.Initiate();
        
        Player.gameObject.SetActive(false);
        Learner.gameObject.SetActive(false);

        AudienceController.Instance.SummonAudience();

        await Task.Delay(3000);

        _isPlayable = true;
        _startTime = Time.realtimeSinceStartup;
        
        StartCoroutine(Player.OnRevive());
        StartCoroutine(Learner.OnRevive());
        
        await Task.Run(NetworkManager.Instance.RunPython);
    }

    private void OnDisable()
    {
        MessageSystem.Unsubscribe(typeof(OnDeathEvent), this);
        MessageSystem.Unsubscribe(typeof(OnReviveEvent), this);
        MessageSystem.Unsubscribe(typeof(OnAttackHitEvent), this);
        MessageSystem.Unsubscribe(typeof(OnAttackGuardEvent), this);
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

        if (_isPlayable)
            _timerText.text = $"{Time.realtimeSinceStartup - _startTime:##00.00}";
    }

    private int _frameCount = 0;

    private void FixedUpdate()
    {
        MaskManager.Instance.Update();
        
        if (RegisteredActors.Count < 2 || !NetworkManager.Instance.isActive) return;
        
        _frameCount++;
        if (_frameCount >= 6f && _frameDataChunk.FindIndex(Time.frameCount) < 0)
        {
            AppendFrameData(new FrameData(Time.frameCount));
            _frameCount = 0;
        }
    }
    
    public void AppendFrameData(FrameData data)
    {
        var idx = _frameDataChunk.FindIndex(data.Frame);
        if (idx < 0)
            _frameDataChunk.Append(data);
        else
            _frameDataChunk[idx] = data;
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
            return OnAttackHitEvent(ahe);
        }
        
        if (e is OnAttackGuardEvent age)
        {
            return OnAttackGuardEvent(age);
        }
        
        return false;
    }
    
    private float hitRewardFactor = 3f;
    private float guardRewardFactor = 2f;

    // 공격 히트 이벤트 처리
    public bool OnAttackHitEvent(OnAttackHitEvent ahe)
    {
        float reward = CalculateHitReward(ahe);
        ApplyRewardToRecentFrames(reward);
        return true;
    }

    // 공격 가드 이벤트 처리
    public bool OnAttackGuardEvent(OnAttackGuardEvent age)
    {
        float reward = CalculateGuardReward(age);
        ApplyRewardToRecentFrames(reward);
        return true;
    }

    // 보상 계산 및 적용
    private void ApplyRewardToRecentFrames(float reward)
    {
        int currentFrame = Time.frameCount;
        AppendFrameData(new FrameData(currentFrame));
        for (int i = 0; i < _frameDataChunk.Frames.Count; i++)
        {
            var frame = _frameDataChunk.Frames[i];
            var elapsed = currentFrame - frame.Frame;
            frame.AddValidation(reward / (60 + elapsed)); // 시간에 따라 감소하는 보상 적용
            _frameDataChunk.Frames[i] = frame; // 수정된 FrameData를 다시 할당
        }
        // Debug.Log($"+> {reward}");
    }

    // 히트 보상 계산
    private float CalculateHitReward(OnAttackHitEvent ahe)
    {
        // 보상 계산 로직
        return ahe.info.Damage * (ahe.takerMask.Owner == Learner ? -0.3f : 1.2f) * hitRewardFactor;
    }

    // 가드 보상 계산
    private float CalculateGuardReward(OnAttackGuardEvent age)
    {
        // 보상 계산 로직
        return age.info.GuardDamage * (age.giverMask.Owner == Learner ? -0.1f : 0.3f) * guardRewardFactor;
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
