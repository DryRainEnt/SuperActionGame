using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

public class BackgroundParallax : MonoBehaviour
{
    private Vector3 _startPos;
    private Transform _cameraTransform;

    [Range(0, 1)]
    public float horizontalRatio;
    [Range(0, 1)]
    public float verticalRatio;

    public float LerpSpeed = 30f;
    private Vector3 parallaxRatio => new Vector3(horizontalRatio, verticalRatio, 1);

    public GameObject Camera;

    private void Start()
    {
        _startPos = transform.position;
        _cameraTransform = Camera.transform;
    }

    private void LateUpdate()
    {
        Vector3 distanceMoved = _cameraTransform.position - _startPos;
        Vector3 targetPos = _startPos + Vector3.Scale(distanceMoved, parallaxRatio);
        
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * LerpSpeed);
    }
}