using System.Collections;
using System.Collections.Generic;
using Proto.PoolingSystem;
using UnityEngine;

public class DeathFX : MonoBehaviour, IPooledObject
{
    private ParticleSystem _particle;
    private float _foreDelay = 0.1f;
    private float _duration = 0.4f;

    private float _innerTimer = 0f;

    public Color Color
    {
        get => _particle.main.startColor.color;
        set
        {
            var main = _particle.main;
            main.startColor = value;
        }
    }

    private void OnEnable()
    {
        _particle = GetComponentInChildren<ParticleSystem>();
    }

    public void Initialize(Color color, float d = 0.6f)
    {
        _particle.Play();
        Color = color;
        _duration = d;
        
        _innerTimer = _duration;
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
        }
    }

    public string Name { get; set; } = "SlashFX";
    
    public void OnPooled()
    {
        _innerTimer = _foreDelay + _duration;
    }

    public void Dispose()
    {
        _particle.Stop();
        _particle.Clear();
        Color = Color.white;
        ObjectPoolController.Dispose(this);
    }
}
