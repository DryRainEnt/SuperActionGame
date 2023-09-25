using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Proto.BasicExtensionUtils;
using Proto.PoolingSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public enum InputDeviceType
{
    Keyboard,
    Gamepad,
}

public class InputData : IDisposable
{
    public string KeyName;
    public float Value;

    private static readonly TinyObjectPool<InputData> pool = new TinyObjectPool<InputData>();
    
    public static InputData Create(string name, float value)
    {
        var e = pool.GetOrCreate();

        e.KeyName = name;
        e.Value = value; 
        
        return e;
    }
    
    public void Dispose()
    {
        pool.Dispose(this);
    }
}

public class ActiveInputDictionary : Dictionary<string, float>
{
    /// <summary>
    /// Add or Update value in input dictionary
    /// </summary>
    /// <param name="data"> Input Data </param>
    /// <returns> Returns true if input is just pressed </returns>
    public bool Add(InputData data)
    {
        if (this.ContainsKey(data.KeyName))
        {
            this[data.KeyName] = data.Value;
            return false;
        }
        else
        {
            this.Add(data.KeyName, data.Value);
            return true;
        }
    }
}

public class InputMapDictionary : Dictionary<string, string>{}

public class GlobalInputController : MonoBehaviour
{
    public static GlobalInputController Instance;

    public InputDeviceType type = InputDeviceType.Keyboard;
    
    
    [SerializeField]
    private UnityEngine.UI.Text _text;

    private ActiveInputDictionary InputDataDictionary;
    public ActiveInputDictionary PressedInputDataDictionary;
    private ActiveInputDictionary ReleasedInputDataDictionary;
    
    private InputMapDictionary _keyboardInputMap = new InputMapDictionary()
    {
        {"left", "kb_a"},
        {"right", "kb_d"},
        {"down", "kb_s"},
        {"up", "kb_w"},
        {"button1", "kb_j"},
        {"button2", "kb_k"},
        {"button3", "kb_l"},
        {"button4", "kb_semicolon"},
        {"jump", "kb_space"},
        {"reset", "kb_r"},
        {"resize", "kb_f5"}
    };
    
    private InputMapDictionary _gamepadInputMap = new InputMapDictionary()
    {
        {"left", "gp_leftStick_left"},
        {"right", "gp_leftStick_right"},
        {"down", "gp_leftStick_down"},
        {"up", "gp_leftStick_up"},
        {"button1", "gp_buttonWest"},
        {"button2", "gp_buttonNorth"},
        {"button3", "gp_buttonEast"},
        {"button4", "gp_rightShoulder"},
        {"jump", "gp_buttonSouth"},
        {"reset", "gp_start"},
        {"resize", "kb_f5"}
    };

    private StringBuilder _stringBuilder;

    private void Awake()
    {
        Instance = this;
        InputDataDictionary = new ActiveInputDictionary();
        PressedInputDataDictionary = new ActiveInputDictionary();
        ReleasedInputDataDictionary = new ActiveInputDictionary();
        _stringBuilder = new StringBuilder();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // var timer = new Timer();
        // timer.Reset();

        _stringBuilder.Clear();
        PressedInputDataDictionary.Clear();
        ReleasedInputDataDictionary.Clear();

#if ENABLE_INPUT_SYSTEM
        // New input system backends are enabled.
        // 추후 InputDeviceType를 사용하여 케이스 분류를 해주어야 한다.
        if (Keyboard.current != null && Keyboard.current.enabled)
            foreach (var input in Keyboard.current.allKeys)
            {
                if (input.wasPressedThisFrame)
                {
                    InputDataDictionary.Add(InputData.Create($"kb_{input.name}", 1f));
                    PressedInputDataDictionary.Add(InputData.Create($"kb_{input.name}", 1f));
                }
                if (!input.isPressed)
                {
                    var rm = InputDataDictionary.Remove($"kb_{input.name}");
                    if (rm) ReleasedInputDataDictionary.Add(InputData.Create($"kb_{input.name}", 1f));
                }
            }

        if (Gamepad.current != null && Gamepad.current.enabled)
            foreach (var input in Gamepad.current.allControls)
            {
                if (input.CheckStateIsAtDefault())
                {
                    if (input.children.Count > 0)
                        foreach (var key in input.children)
                        {
                            var ik = $"gp_{input.name}_{key.name}";
                            if (!InputDataDictionary.Remove(ik)) continue;
                            ReleasedInputDataDictionary.Add(InputData.Create(ik, 1f));
                        }
                    else
                    {
                        var ik = $"gp_{input.name}";
                        if (!InputDataDictionary.Remove(ik)) continue;
                        ReleasedInputDataDictionary.Add(InputData.Create(ik, 1f));
                    }
                }
                else
                {
                    if (input.children.Count > 0)
                        foreach (var key in input.children)
                        {
                            InputDataDictionary.Add(InputData.Create($"gp_{input.name}_{key.name}", key.EvaluateMagnitude()));
                            if (input.EvaluateMagnitude() > 0.7f)
                                PressedInputDataDictionary.Add(InputData.Create($"gp_{input.name}_{key.name}", key.EvaluateMagnitude()));
                        }
                    else
                    {
                        InputDataDictionary.Add(InputData.Create($"gp_{input.name}", input.EvaluateMagnitude()));
                        if (input.EvaluateMagnitude() > 0.7f)
                            PressedInputDataDictionary.Add(InputData.Create($"gp_{input.name}", input.EvaluateMagnitude()));
                    }
                }
                
            }

        foreach (var inputPair in InputDataDictionary)
        {
            _stringBuilder.AppendLine($"[{inputPair.Key} : {inputPair.Value}]");
        }
        
            
#endif

        _text.text = _stringBuilder.ToString();
        
        // timer.Stop();
        // Debug.Log($"Input Checker Calc Time : {timer}");
        // timer.Dispose();
    }

    public InputDeviceType ToggleInputDevice()
    {
        switch (type)
        {
            case InputDeviceType.Keyboard:
                type = InputDeviceType.Gamepad;
                break;
            case InputDeviceType.Gamepad:
                type = InputDeviceType.Keyboard;
                break;
        }

        return type;
    }

    public string GetKeyMapping(string target)
    {
        switch (type)
        {
            case InputDeviceType.Keyboard:
                if (!_keyboardInputMap.ContainsKey(target))
                    break;
                return _keyboardInputMap[target];
            case InputDeviceType.Gamepad:
                if (!_gamepadInputMap.ContainsKey(target))
                    break;
                return _gamepadInputMap[target];
        }

        return "null";
    }

    public void ModifyKeyMapping(string key, string value)
    {
        switch (type)
        {
            case InputDeviceType.Keyboard:
                if (!_keyboardInputMap.ContainsKey(key))
                    return;
                _keyboardInputMap[key] = value;
                break;
            case InputDeviceType.Gamepad:
                if (!_gamepadInputMap.ContainsKey(key))
                    return;
                _gamepadInputMap[key] = value;
                break;
        }
        
    }
    
    public void StartKeyMapping(Action<string> callback)
    {
        if (OnKeyMapping) return;
        StartCoroutine(KeyMapping(callback));
    }
    
    private bool OnKeyMapping { get; set; }
    
    private IEnumerator KeyMapping(Action<string> callback)
    {
        var target = "";
        OnKeyMapping = true;
        while (target.Length == 0)
        {
            switch (type)
            {
                case InputDeviceType.Keyboard when (Keyboard.current != null && Keyboard.current.enabled):
                {
                    foreach (var input in Keyboard.current.allKeys)
                    {
                        if (input.wasPressedThisFrame)
                            target = Utils.BuildString("kb_", input.name);
                    }

                    break;
                }
                case InputDeviceType.Gamepad when (Gamepad.current != null && Gamepad.current.enabled):
                {
                    foreach (var input in Gamepad.current.allControls)
                    {
                        if (input.CheckStateIsAtDefault()) continue;
                        if (input.children.Count > 0)
                            foreach (var key in input.children)
                            {
                                if (key.EvaluateMagnitude() > 0.9f)
                                    target = Utils.BuildString("gp_", input.name, "_", key.name);
                            }
                        else if (input.EvaluateMagnitude() > 0.9f)
                            target = Utils.BuildString("gp_", input.name);

                    }

                    break;
                }
            }

            yield return null;
        }

        OnKeyMapping = false;
        callback(target);
    }

    public bool GetReleased(string key)
    {
        return key switch
        {
            "horizontal" => (GetValue("right") - GetValue("left")).GetFilteredFloat(0.1f),
            "vertical" => GetValue("up") - GetValue("down"),
            _ => type switch
            {
                InputDeviceType.Keyboard => GetDirectReleased(_keyboardInputMap[key]),
                InputDeviceType.Gamepad => GetDirectReleased(_gamepadInputMap[key]),
                _ => 0f
            }
        } > 0.5f;
    }
    public bool GetPressed(string key)
    {
        return key switch
        {
            "horizontal" => (GetValue("right") - GetValue("left")).GetFilteredFloat(0.1f),
            "vertical" => GetValue("up") - GetValue("down"),
            _ => type switch
            {
                InputDeviceType.Keyboard => GetDirectPressed(_keyboardInputMap[key]),
                InputDeviceType.Gamepad => GetDirectPressed(_gamepadInputMap[key]),
                _ => 0f
            }
        } > 0.5f;
    }
    public bool GetInput(string key)
    {
        return key switch
        {
            "horizontal" => (GetValue("right") - GetValue("left")).GetFilteredFloat(0.1f),
            "vertical" => GetValue("up") - GetValue("down"),
            _ => type switch
            {
                InputDeviceType.Keyboard => GetDirectValue(_keyboardInputMap[key]),
                InputDeviceType.Gamepad => GetDirectValue(_gamepadInputMap[key]),
                _ => 0f
            }
        } > 0.5f;
    }
    
    public float GetValue(string key)
    {
        return key switch
        {
            "horizontal" => (GetValue("right") - GetValue("left")).GetFilteredFloat(0.1f),
            "vertical" => GetValue("up") - GetValue("down"),
            _ => type switch
            {
                InputDeviceType.Keyboard => GetDirectValue(_keyboardInputMap[key]),
                InputDeviceType.Gamepad => GetDirectValue(_gamepadInputMap[key]),
                _ => 0f
            }
        };
    }
    
    public float GetDirectPressed(string key)
    {
        return PressedInputDataDictionary.ContainsKey(key) ? PressedInputDataDictionary[key] : 0f;
    }
    
    public float GetDirectReleased(string key)
    {
        return ReleasedInputDataDictionary.ContainsKey(key) ? ReleasedInputDataDictionary[key] : 0f;
    }
    
    public float GetDirectValue(string key)
    {
        return InputDataDictionary.ContainsKey(key) ? InputDataDictionary[key] : 0f;
    }
}
