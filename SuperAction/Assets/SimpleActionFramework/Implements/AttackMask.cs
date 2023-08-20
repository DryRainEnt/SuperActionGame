using System.Collections.Generic;
using Proto.PoolingSystem;
using UnityEngine;

public class AttackMask : MonoBehaviour, IPooledObject
{
    public Actor container { get; private set; }
    
    private BoxCollider2D _col;
    private SpriteRenderer _sr;

    private List<Actor> _hitThisFrame = new List<Actor>();
    private List<Actor> _hitRecord = new List<Actor>();
    public Actor firstHit { get; private set; }

    public DamageInfo Info;

    public float _innerTimer;
    
    private void Awake()
    {
        _col = GetComponent<BoxCollider2D>();
        _sr = GetComponent<SpriteRenderer>();
        _sr.enabled = false;
    }

    public void SetMask(Bounds bound, Actor character)
    {
        container = character;
        Transform.localScale = bound.size;
        Transform.localPosition = bound.center;
    }

    // Update is called once per frame
    void Update()
    {
        var dt = Time.deltaTime;

        _hitThisFrame.Clear();

        _innerTimer += dt;
    }

    public void ResetHitRecord()
    {
        _hitRecord.Clear();
        _hitThisFrame.Clear();
        firstHit = null;
    }

    public void ResetHitRecordByLifetime(float time)
    {
        if (_innerTimer > time)
        {
            ResetHitRecord();
            _innerTimer = 0f;
        }
    }

    public Transform Transform => transform;

    public Vector3 Position
    {
        get => transform.position; 
        set => transform.position = value;
    }

    public Bounds Bound
    {
        get => _col.bounds;
        set => _col.bounds.SetMinMax(value.min, value.max);
    }
    public string Name { get; set; } = "AttackMask";
    
    public void OnPooled()
    {
        #if UNITY_EDITOR
        _sr.enabled = true;
        #endif
        _innerTimer = 0f;
        Bound = new Bounds(Vector2.zero, new Vector2(1.5f, 2f));
        ResetHitRecord();
    }

    public void Dispose()
    {
        ResetHitRecord();
        Transform.localScale = Vector3.one;
        Transform.localPosition = Vector3.zero;
        _innerTimer = 0f;
        Info = null;
        ObjectPoolController.Self.Dispose(this);
    }
}
