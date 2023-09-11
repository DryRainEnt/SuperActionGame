using System;
using System.Collections;
using System.Collections.Generic;
using Proto.PoolingSystem;
using TMPro;
using UnityEngine;

public class DamageTextFX : MonoBehaviour, IPooledObject
{
    private TMP_Text _text;
    private float _foreDelay = 0.3f;
    private float _duration = 1f;

    private float _innerTimer = 0f;

    public Vector2 Velocity;
    
    public string Text
    {
        get => _text.text;
        set => _text.text = value;
    }

    public Color Color
    {
        get => _text.color;
        set => _text.color = value;
    }

    private void OnEnable()
    {
        _text = GetComponentInChildren<TMP_Text>();
    }

    public void Initialize(string t, Vector2 direction, float f = 0.3f, float d = 1f)
    {
        Text = t;
        Velocity = direction;
        _foreDelay = f;
        _duration = d;
        
        _innerTimer = _foreDelay + _duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (_innerTimer > 0)
        {
            _innerTimer -= Time.deltaTime;
            if (_innerTimer <= 0)
            {
                Dispose();
            }
            else if (_innerTimer <= _duration)
            {
                transform.position += (Vector3)Velocity * Time.deltaTime;
                Velocity += Vector2.down * (Time.deltaTime * 9.8f);
            }
            else
            {
                transform.position += (Vector3)Velocity * (0.3f * Time.deltaTime);
            }
        }
    }

    public string Name { get; set; } = "DamageTextFX";
    
    public void OnPooled()
    {
        _innerTimer = _foreDelay + _duration;
    }

    public void Dispose()
    {
        Text = "";
        Velocity = Vector2.zero;
        ObjectPoolController.Dispose(this);
    }
}
