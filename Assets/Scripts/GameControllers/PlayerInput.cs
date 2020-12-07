using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GolfBall))]
public class PlayerInput : MonoBehaviour
{
    private GolfBall _golfBall;
    public float maxDistance;
    public float maxForce;
    private bool _canSwing;
    private bool _beginSwing;

    void Awake()
    {
        _golfBall = GetComponent<GolfBall>();
        _canSwing = true;
    }

    void Start()
    {
        HUDController.SetPowerLevel(0);
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
                float powerLevel = Mathf.Clamp01(displacement.magnitude / maxDistance);
                HUDController.SetPowerLevel(powerLevel);

                if (Input.GetMouseButtonUp(0))
                {
                    Vector3 direction = displacement.normalized;
                    _golfBall.ApplyForce(direction * maxForce * powerLevel);
                    _beginSwing = false;
                }
            }
        }
    }

    public void SetCanSwing(bool value)
    {
        _canSwing = value;
        if (_canSwing)
        {
            HUDController.SetPowerLevel(0);
        }
        else 
        {
            _beginSwing = false;
        }
    }
}
