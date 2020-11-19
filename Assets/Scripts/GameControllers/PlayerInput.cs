using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GolfBall))]
public class PlayerInput : MonoBehaviour
{
    private GolfBall _golfBall;
    public float force;

    void Awake()
    {
        _golfBall = GetComponent<GolfBall>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, transform.position);
            if (plane.Raycast(ray, out float enter))
            {

                Vector3 point = ray.GetPoint(enter);
                Vector3 displacement = point - transform.position;
                Vector3 direction = displacement.normalized;

                _golfBall.ApplyForce(direction * force);
            }
        }
    }

}
