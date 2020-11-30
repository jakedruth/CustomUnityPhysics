using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed;
    public Transform target;

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, moveSpeed * Time.deltaTime);
    }
}
