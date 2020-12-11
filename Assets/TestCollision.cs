using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCollision : MonoBehaviour
{
    public MyRigidBody sphereA;
    public MyRigidBody sphereB;
    public MyRigidBody boxA;
    public Transform plane;

    // Update is called once per frame
    void Update()
    {
        Primitive.Sphere sA = new Primitive.Sphere(sphereA, Vector3.zero, Quaternion.identity, 0.5f);
        Primitive.Sphere sB = new Primitive.Sphere(sphereB, Vector3.zero, Quaternion.identity, 0.5f);
        Primitive.Box bA = new Primitive.Box(boxA, Vector3.zero, Quaternion.identity, Vector3.one * 0.5f);
        Primitive.Plane p = new Primitive.Plane(plane.up, (plane.position).magnitude);

        CollisionDetector.CollisionData data = new CollisionDetector.CollisionData(0, 0);
        int contactCount = 0;
        contactCount += CollisionDetector.SphereAndSphere(sA, sB, ref data);
        contactCount += CollisionDetector.SphereAndHalfSpace(sA, p, ref data);
        contactCount += CollisionDetector.SphereAndHalfSpace(sB, p, ref data);

        contactCount += CollisionDetector.BoxAndSphere(bA, sA, ref data);
        contactCount += CollisionDetector.BoxAndSphere(bA, sB, ref data);
        contactCount += CollisionDetector.BoxAndHalfSpace(bA, p, ref data);

        Debug.Log(contactCount);

        foreach (CollisionDetector.Contact contact in data.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal * contact.penetration, Color.red);
        }
    }
}
