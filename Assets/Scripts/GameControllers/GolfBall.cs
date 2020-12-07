using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GolfBall : MonoBehaviour
{
    //private Rigidbody _rigidbody;
    private MyRigidBody _myRigidBody;


    void Awake()
    {
        //_rigidbody = GetComponent<Rigidbody>();
        _myRigidBody = GetComponent<MyRigidBody>();
    }

    void FixedUpdate()
    {

    }

    public void ApplyForce(Vector3 force)
    {
        //_rigidbody.AddForce(maxForce);
        _myRigidBody.AddForceAtPoint(force, transform.position + Vector3.up);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Handles.PositionHandle(transform.position, transform.rotation);
#endif
    }


}
