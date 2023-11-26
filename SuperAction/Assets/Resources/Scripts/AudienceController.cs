using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudienceController : MonoBehaviour
{
    private static AudienceController _instance;
    public static AudienceController Instance => _instance ? _instance : _instance = FindObjectOfType<AudienceController>();

    private Animator[] Audiences;
    private SpriteRenderer[] Sprites;
    private LineRenderer[] Lines;
    
    [SerializeField]
    private int[] AnimIds;
    [SerializeField]
    private float[] AnimVariances = new float[5]
    {
        1f, 1f, 1f, 1f, 1f
    };

    private void Awake()
    {
        var count = transform.childCount;
        Audiences = new Animator[count];
        Sprites = new SpriteRenderer[count];
        Lines = new LineRenderer[count];
        AnimIds = new int[count];
        
        for (int i = 0; i < count; i++)
        {
            var anim = transform.GetChild(i).GetComponent<Animator>();
            Audiences[i] = anim;
            Sprites[i] = anim.GetComponent<SpriteRenderer>();
            Lines[i] = anim.GetComponent<LineRenderer>();

            Sprites[i].enabled = false;
            Lines[i].enabled = false;
        }
    }

    public void SummonAudience()
    {
        for (int i = 0; i < 64; i++)
        {
            var index = GetRandomIndex();
            StartCoroutine(SetAudience(i, index + 1));
        }
    }

    private int GetRandomIndex(float variance = 1f)
    {
        var weight = AnimVariances.Sum();

        var rand = UnityEngine.Random.value;
        var marker = 0f;
        var result = 0;
        for (int i = 0; i < AnimVariances.Length; i++)
        {
            var weightRatio = AnimVariances[i] / weight;
            marker += weightRatio;
            if (rand <= marker)
            {
                result = i;
                break;
            }
        }
        
        for (int i = 0; i < AnimVariances.Length; i++)
        {
            if (i != result)
                AnimVariances[i] += variance;
        }

        return result;
    }

    public void DisposeAudience()
    {
        for (int i = 0; i < 64; i++)
        {
            var index = Audiences[i];
            var sprite = Sprites[i];
            sprite.enabled = false;
        }
    }

    private IEnumerator SetAudience(int index, int animId = -1)
    {
        var anim = Audiences[index];
        var sprite = Sprites[index];
        var line = Lines[index];
        
        animId = animId == -1 ? UnityEngine.Random.Range(1, 6) : animId;
        
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 1.3f));
        
        var randomColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
        
        line.enabled = true;
        line.startColor = randomColor;
        line.endColor = randomColor;
        line.startWidth = 0.2f;
        line.endWidth = 0.2f;
        
        sprite.enabled = true;
        sprite.color = randomColor;
        Audiences[index].Play("Aud" + animId);
        AnimIds[index] = animId;

        var timer = 0.3f;
        while (timer > 0f)
        {
            var progress = timer / 0.3f;
            var pos = anim.transform.position;
            
            line.SetPosition(0, pos);
            line.SetPosition(1, pos + Vector3.up * 128f);
            
            line.startWidth = 0.2f * progress;
            line.endWidth = 0.2f * progress;
            timer -= Time.deltaTime;
            yield return null;
        }
        
        line.startWidth = 0f;
        line.endWidth = 0f;
        line.enabled = false;
    }
}
