using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float lerpSpeed;
    public Transform target;
    public float threshold;

    void Update()
    {
        Vector3 displacement = target.position - transform.position;

        transform.position = displacement.sqrMagnitude > threshold * threshold
            ? Vector3.Lerp(transform.position, target.position, lerpSpeed * Time.deltaTime) 
            : target.position;
    }
}
