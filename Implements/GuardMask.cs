using Proto.PoolingSystem;
using UnityEngine;

public class GuardMask : MonoBehaviour, IPooledObject
{
    public Actor container { get; private set; }
    
    private BoxCollider2D _col;
    private SpriteRenderer _sr;

    private float _limit;

    private void Awake()
    {
        _col = GetComponent<BoxCollider2D>();
        _sr = GetComponent<SpriteRenderer>();
        _sr.enabled = false;
    }

    // 가드는 능동적으로 행동하진 않기 때문에 Update를 쓰지 않는다.

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
    
    public string Name { get; set; } = "GuardMask";
    
    public void SetMask(Bounds bound, Actor character, float limit)
    {
        container = character;
        Transform.localScale = bound.size;
        Transform.localPosition = bound.center;
        _limit = limit;
    }

    public void Guard(DamageInfo info)
    {
    }

    public void OnPooled()
    {
#if UNITY_EDITOR
        _sr.enabled = true;
#endif
        Bound = new Bounds(Vector2.zero, new Vector2(1.5f, 2f));
    }

    public void Dispose()
    {
        Transform.localScale = Vector3.one;
        Transform.localPosition = Vector3.zero;
        _limit = 1f;
        ObjectPoolController.Self.Dispose(this);
    }
}
