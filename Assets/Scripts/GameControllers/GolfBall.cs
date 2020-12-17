using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GolfBall : MonoBehaviour
{
    private MyRigidBody _myRigidBody;
    public bool isBall;

    void Awake()
    {
        _myRigidBody = GetComponent<MyRigidBody>();

        if (isBall)
        {
            _myRigidBody.SetInertiaTensor(MyPhysics.GetInertiaTensorSphere(_myRigidBody.mass, 0.5f));
            Primitive.Sphere sphere = new Primitive.Sphere(_myRigidBody, Vector3.zero, Quaternion.identity, 0.5f);
            WorldManager.Spheres.Add(sphere);
        }
        else
        {
            _myRigidBody.SetInertiaTensor(MyPhysics.GetInertiaTensorCuboid(_myRigidBody.mass, 1, 1, 1));
            Primitive.Box box = new Primitive.Box(_myRigidBody, Vector3.zero, Quaternion.identity, Vector3.one * 0.5f);
            WorldManager.Boxes.Add(box);

            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);

            GetComponent<MeshFilter>().mesh = temp.GetComponent<MeshFilter>().sharedMesh;

            Destroy(temp);
        }

        WorldManager.Bodies.Add(_myRigidBody);
        _myRigidBody.SetAwake();
    }

    public void ApplyForce(Vector3 force)
    {
        _myRigidBody.AddForce(force, transform.position + Vector3.up);
    }
}
