using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GolfBall))]
[RequireComponent(typeof(LineRenderer))]
public class PlayerInput : MonoBehaviour
{
    private GolfBall _golfBall;
    private LineRenderer _lineRenderer;
    public Gradient powerLevelColorGradient;

    public float maxDistance;
    public float maxForce;
    private bool _canSwing;
    private bool _beginSwing;

    void Awake()
    {
        _golfBall = GetComponent<GolfBall>();
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
        _canSwing = true;
    }

    void Start()
    {
        HUDController.SetPowerLevel(0, Color.white);
    }

    void Update()
    {
        Vector3 pos = transform.position + Vector3.down * 0.5f;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, pos);
        if (!plane.Raycast(ray, out float enter)) 
            return;

        Vector3 point = ray.GetPoint(enter);

        if (_canSwing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _beginSwing = true;
            }

            if (_beginSwing)
            {
                Vector3 displacement = point - pos;
                (Vector3 direction, float magnitude) tuple = displacement.GetDirectionAndMagnitude();
                float powerLevel = Mathf.Clamp01(tuple.magnitude / maxDistance);
                Color color = powerLevelColorGradient.Evaluate(powerLevel);
                HUDController.SetPowerLevel(powerLevel, color);

                SetArrow(tuple.direction * maxDistance * powerLevel, color);

                if (Input.GetMouseButtonUp(0))
                {
                    _golfBall.ApplyForce(tuple.direction * maxForce * powerLevel);
                    _beginSwing = false;
                    _lineRenderer.positionCount = 0;
                }
            }
        }
    }

    public void SetCanSwing(bool value)
    {
        _canSwing = value;
        if (_canSwing)
        {
            HUDController.SetPowerLevel(0, Color.white);
        }
        else 
        {
            _beginSwing = false;
        }
    }

    public void SetArrow(Vector3 direction, Color color)
    {
        float adaptiveSize = 1f / direction.magnitude;
        float lineWidth = 0.2f;

        Vector3 arrowOrigin = transform.position + Vector3.down * 0.3f;
        Vector3 arrowTarget = arrowOrigin + direction;

        _lineRenderer.positionCount = 4;
        
        _lineRenderer.widthCurve = new AnimationCurve(
            new Keyframe(0, lineWidth)
            , new Keyframe(0.999f - adaptiveSize, lineWidth)  // neck of arrow
            , new Keyframe(1 - adaptiveSize, 1f)  // max width of arrow head
            , new Keyframe(1, 0f));  // tip of arrow
        _lineRenderer.SetPositions(new Vector3[] {
            arrowOrigin
            , Vector3.Lerp(arrowOrigin, arrowTarget, 0.999f - adaptiveSize)
            , Vector3.Lerp(arrowOrigin, arrowTarget, 1 - adaptiveSize)
            , arrowTarget });

        Gradient g = new Gradient();
        g.SetKeys(
            new []{new GradientColorKey(color, 0)},
            new []{new GradientAlphaKey(1, 0) });

        _lineRenderer.colorGradient = g;
    }
}
