using System.Collections;
using System.Collections.Generic;
using Proto.BasicExtensionUtils;
using Proto.PoolingSystem;
using UnityEngine;

public class SlashFX : MonoBehaviour, IPooledObject
{
    private LineRenderer _lr;
    private float _foreDelay = 0.1f;
    private float _duration = 0.4f;

    private float _innerTimer = 0f;

    public Vector2 Direction;

    public Vector2 Position => (Vector2)transform.position;
    
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

    public void Initialize(float w, Vector2 direction, Color color, float f = 0.1f, float d = 0.4f)
    {
        Width = w;
        Direction = direction;
        _lr.positionCount = 2;
        _lr.SetPosition(0, Position - Direction.normalized * 1920f);
        _lr.SetPosition(1, Position + Direction.normalized * 1920f);
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
                Width = Mathf.Lerp(Width, 0, _innerTimer);
            }
        }
    }

    public string Name { get; set; } = "SlashFX";
    
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
