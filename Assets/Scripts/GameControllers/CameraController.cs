using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Holder Values")]
    public float minFieldOfViewRange;
    public float maxFieldOfViewRange;
    public float minDeltaYRange;
    public float maxDeltaYRange;
    public float minDeltaZRange;
    public float maxDeltaZRange;

    [Header("Camera input values")]
    public float scrollSpeed;
    public bool invertScroll;
    public float rotateSpeed;
    public bool invertRotate;

    [Range(0f, 1f)] public float t;

    [Header("Camera Move Values")] public Transform targetTransform;
    public float cameraMoveSpeed;

    private Transform _camHolder;
    public Transform CamHolderTransform
    {
        get { return _camHolder = _camHolder == null ? transform.GetChild(0) : _camHolder; }

    }

    private Camera _cam;
    public Camera Cam
    {
        get { return _cam = _cam == null ? transform.GetComponentInChildren<Camera>() : _cam; }
    }

    private Vector3 _mouseDownLast;

    public void Update()
    {
        Vector3 mouseCurrent = Cam.ScreenToViewportPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(1))
        {
            _mouseDownLast = mouseCurrent;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = _mouseDownLast - mouseCurrent;
            MoveRotation(delta.x * (invertRotate ? -rotateSpeed : rotateSpeed));
            MoveT(delta.y * (invertScroll ? -scrollSpeed : scrollSpeed) * Time.deltaTime );

            _mouseDownLast = mouseCurrent;
        }
    }

    public void FixedUpdate()
    {
        UpdateCameraHolder();
        transform.position =
            Vector3.Lerp(transform.position, targetTransform.position, cameraMoveSpeed * Time.deltaTime);
    }

    public void OnValidate()
    {
        UpdateCameraHolder();
    }

    public void MoveTargetTransfromPosition(Vector3 delta)
    {
        delta.y = 0;
        targetTransform.position += Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * (delta);
    }

    public void SetTargetTransfomPosition(Vector3 position)
    {
        position.y = 0;
        targetTransform.position = position;
    }

    public void MoveRotation(float delta)
    {
        transform.Rotate(Vector3.up, delta);
    }

    public void SetRotation(float value)
    {
        transform.rotation = Quaternion.AngleAxis(value, Vector3.up);
    }

    public void MoveT(float delta)
    {
        t = Mathf.Clamp01(t + delta);
    }

    public void SetT(float value)
    {
        t = Mathf.Clamp01(value);
    }

    private void UpdateCameraHolder()
    {
        Vector3 a = new Vector3(0, Mathf.Lerp(minDeltaYRange, maxDeltaYRange, t), minDeltaZRange);
        Vector3 b = new Vector3(0, maxDeltaYRange, Mathf.Lerp(maxDeltaZRange, maxDeltaZRange, t));
        Vector3 position = Vector3.Lerp(a, b, t);
        CamHolderTransform.localPosition = position;

        CamHolderTransform.transform.LookAt(transform);

        Cam.fieldOfView = Mathf.Lerp(minFieldOfViewRange, maxFieldOfViewRange, t);
    }
}
