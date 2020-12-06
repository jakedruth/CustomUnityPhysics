using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GolfBall : MonoBehaviour
{
    //private Rigidbody _rigidbody;
    private MyRigidBody _myRigidBody;

    // TODO: Remove this
    private AnchoredSpringFG spring;
    private GravityFG gravity;

    void Awake()
    {
        //_rigidbody = GetComponent<Rigidbody>();
        _myRigidBody = GetComponent<MyRigidBody>();

        spring = new AnchoredSpringFG(new Vector3(0, 3, 0), new Vector3(0, 0.5f, 0), 5, 1);
        gravity = new GravityFG(Vector3.down * 2);
    }

    void FixedUpdate()
    {
        gravity.UpdateForce(_myRigidBody, Time.deltaTime);
        spring.UpdateForce(_myRigidBody, Time.deltaTime);

        Debug.DrawLine(spring.anchor, transform.TransformPoint(spring.connectionPoint), Color.red, Time.deltaTime, false);
        Debug.DrawRay(transform.position, transform.right, Color.red, Time.deltaTime, false);
        Debug.DrawRay(transform.position, transform.up, Color.green, Time.deltaTime, false);
        Debug.DrawRay(transform.position, transform.forward, Color.blue, Time.deltaTime, false);

    }

    public void ApplyForce(Vector3 force)
    {
        //_rigidbody.AddForce(force);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Handles.PositionHandle(transform.position, transform.rotation);
#endif
    }


}
