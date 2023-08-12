using System;
using System.Collections;
using System.Collections.Generic;
using SimpleActionFramework.Core;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class KeyMapButtonDictionary : SerializedDictionary<Button, string>{}

public class KeyMapper : MonoBehaviour
{
    public KeyMapButtonDictionary Buttons;

    private Button _currentButton = null;
    private string CurrentTarget => Buttons[_currentButton] ?? "null";


    public Button InputDeviceToggle;

    private void Start()
    {
        foreach (var button in Buttons.Keys)
        {
            button.onClick.AddListener(() => KeyMapping(button));
            SetButtonString(button);
        }
        InputDeviceToggle.onClick.AddListener(ToggleInputDevice);
        ToggleInputDevice();
        ToggleInputDevice();
    }

    private void ToggleInputDevice()
    {
        var type = GlobalInputController.Instance.ToggleInputDevice();
        foreach (var button in Buttons.Keys)
        {
            SetButtonString(button);
        }
        
        InputDeviceToggle.image.color = (type == InputDeviceType.Keyboard) ? Color.yellow : Color.green;
        var text = InputDeviceToggle.GetComponentInChildren<Text>();
        text.text = (type == InputDeviceType.Keyboard) ? "Keyboard" : "GamePad";
    }

    private void SetButtonString(Button button)
    {
        var text = button.GetComponentInChildren<Text>();
        text.text = Utils.BuildString("[", Buttons[button], "] : ", 
            GlobalInputController.Instance.GetKeyMapping(Buttons[button]));
    }

    private void KeyMapping(Button target)
    {
        if (!Buttons.ContainsKey(target)) return;
        
        var input = GlobalInputController.Instance;
        
        _currentButton = target;
        input.StartKeyMapping(ApplyKeyButton);
    }

    private void ApplyKeyButton(string input)
    {
        GlobalInputController.Instance.ModifyKeyMapping(CurrentTarget, input);
        SetButtonString(_currentButton);
        _currentButton = null;
    }
}
