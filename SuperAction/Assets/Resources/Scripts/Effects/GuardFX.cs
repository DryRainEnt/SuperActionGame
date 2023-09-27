using System.Collections;
using System.Collections.Generic;
using Proto.BasicExtensionUtils;
using Proto.PoolingSystem;
using UnityEngine;

public class GuardFX : MonoBehaviour, IPooledObject
{
    private LineRenderer _lr;
    private float _foreDelay = 0.12f;
    private float _duration = 0.24f;

    private float _innerTimer = 0f;

    public Vector2 Position => (Vector2)transform.position;

    private Vector2[] _localPos = 
    {
        new Vector2(0f, 1f),
        new Vector2(0.8f, 0.5f),
        new Vector2(0.8f, -0.5f),
        new Vector2(0f, -1f),
        new Vector2(-0.8f, -0.5f),
        new Vector2(-0.8f, 0.5f),
    };
    
    public float Width
    {
        get => _lr.endWidth;
        set
        {
            _lr.endWidth = value;
            _lr.startWidth = value;
        }
    }

    public Color Color
    {
        get => _lr.endColor;
        set
        {
            _lr.endColor = value;
            _lr.startColor = value;
        }
    }

    private void OnEnable()
    {
        _lr = GetComponentInChildren<LineRenderer>();
    }

    public void Initialize(Color color, float f = 0.12f, float d = 0.24f)
    {
        _lr.positionCount = 6;
        Width = 0.03f;
        for(int i = 0; i < 6; i++)
        {
            _lr.SetPosition(i, Position + _localPos[i] * 0.1f);
        }
        Color = color;
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
                Width = Mathf.Lerp(0f, 0.4f, _innerTimer / _duration);
                for(int i = 0; i < 6; i++)
                {
                    _lr.SetPosition(i, Position + _localPos[i] * 0.4f);
                }
            }
            else
            {
                Width = Mathf.Lerp(0.4f, 0.1f, (_innerTimer - _duration) / _foreDelay);
                for(int i = 0; i < 6; i++)
                {
                    _lr.SetPosition(i, Position + _localPos[i] * (Width));
                }
            }
        }
    }

    public string Name { get; set; } = "GuardFX";
    
    public void OnPooled()
    {
        _innerTimer = _foreDelay + _duration;
    }

    public void Dispose()
    {
        Width = 0;
        Color = Color.white;
        ObjectPoolController.Dispose(this);
    }
}
