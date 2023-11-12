using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LabelListener : MonoBehaviour
{
    public TMP_Text Label => _label ? _label : _label = GetComponent<TMP_Text>();
    private TMP_Text _label;

    public string postFix;
    public string preFix;
    public string format;

    public void SetLabel(string text)
    {
        Label.text = Utils.BuildString(preFix, text, postFix);
    }

    public void ListenFloat(Single value)
    {
        var str = $"{value}";
        SetLabel(str);
    }
    
    public void ListenInt(int value)
    {
        var str = $"{value}";
        SetLabel(str);
    }
}
